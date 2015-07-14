﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Messages;
using SuperGlue.Logging;

namespace SuperGlue.EventStore.Subscribers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeSubscribers : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILog _log;
        private static bool running;
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions = new Dictionary<string, IServiceSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeSubscribers(IHandleEventSerialization eventSerialization, IWriteToErrorStream writeToErrorStream, IEventStoreConnection eventStoreConnection, ILog log)
        {
            _eventSerialization = eventSerialization;
            _writeToErrorStream = writeToErrorStream;
            _eventStoreConnection = eventStoreConnection;
            _log = log;

            _dispatchWaitSeconds = 1;

            int dispatchWaitSeconds;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.WaitSeconds"] ?? "", out dispatchWaitSeconds))
                _dispatchWaitSeconds = dispatchWaitSeconds;

            _numberOfEventsPerBatch = 256;

            int numberOfEventsPerBatch;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.NumberOfEventsPerBatch"] ?? "", out numberOfEventsPerBatch))
                _numberOfEventsPerBatch = numberOfEventsPerBatch;
        }

        public string Chain { get { return "chains.Subscribers"; } }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            _log.Info("Starting subscribers for environment: {0}", environment);

            running = true;

            var streams = (ConfigurationManager.AppSettings["EventStore.Streams"] ?? "").Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();

            foreach (var stream in streams)
                await SubscribeService(chain, stream, settings);
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            _log.Info("Shutting down subscribers");

            running = false;

            foreach (var subscription in _serviceSubscriptions)
                subscription.Value.Close();

            _serviceSubscriptions.Clear();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            _log.Info("Getting default chain for subscribers for environment: {0}", environment);
            return buildApp.Use<ExecuteSubscribers>().Use<SetLastEvent>().Build();
        }

        protected virtual void OnServiceError(string stream, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ServiceEventProcessingFailed(stream, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
            _log.Error(exception, "Error while processing event of type: {0} for stream: {1}", message != null ? message.GetType().FullName : "Unknown", stream);
        }

        private async Task SubscribeService(AppFunc chain, string stream, IDictionary<string, object> environment)
        {
            var liveOnlySubscriptions = ConfigurationManager.AppSettings["Service.Subscription.LiveOnly"] == "true";
            if (!running)
                return;

            _log.Info("Subscribing to stream: {0}", stream);

            while (true)
            {
                try
                {
                    await SubscribeService(chain, stream, liveOnlySubscriptions, environment);
                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    _log.Error(ex, "Couldn't subscribe to stream: {0}. Retrying in 500 ms.", stream);

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
        }

        private async Task SubscribeService(AppFunc chain, string stream, bool liveOnlySubscriptions, IDictionary<string, object> environment)
        {
            var subscriptionKey = stream;

            if (_serviceSubscriptions.ContainsKey(subscriptionKey))
            {
                _serviceSubscriptions[subscriptionKey].Close();
                _serviceSubscriptions.Remove(subscriptionKey);
            }

            var messageProcessor = new MessageProcessor();

            var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(_dispatchWaitSeconds), _numberOfEventsPerBatch)
                        .Subscribe(async x => await PushEventsToService(chain, stream, x, !liveOnlySubscriptions, environment));

            if (liveOnlySubscriptions)
            {
                var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamAsync(stream, true,
                    (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                    async (subscription, reason, exception) => await SubscriptionDropped(chain, stream, true, reason, exception, environment)).Result;

                _serviceSubscriptions[subscriptionKey] = new LiveOnlyServiceSubscription(messageSubscription, eventstoreSubscription);
            }
            else
            {
                var manageStreamEventNumbers = environment.Resolve<IManageEventNumbersForSubscriber>();

                var lastEvent = await manageStreamEventNumbers.GetLastEvent(stream, environment);

                var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(stream, lastEvent, true,
                    (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                    subscriptionDropped:
                        (subscription, reason, exception) => SubscriptionDropped(chain, stream, false, reason, exception, environment));

                _serviceSubscriptions[subscriptionKey] = new CatchUpServiceSubscription(messageSubscription, eventstoreSubscription);
            }
        }

        private async Task SubscriptionDropped(AppFunc chain, string stream, bool liveOnlySubscriptions, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            _log.Warn(exception, "Subscription dropped for stream: {0}. Reason: {1}. Retrying...", stream, reason);

            await SubscribeService(chain, stream, liveOnlySubscriptions, environment);
        }

        private async Task PushEventsToService(AppFunc chain, string stream, IEnumerable<DeSerializationResult> events, bool catchup, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var failedEvents = eventsList.Where(x => !x.Successful);
            foreach (var evnt in failedEvents)
                OnServiceError(stream, evnt.Data, evnt.Metadata, evnt.Error);

            var successfullEvents = eventsList
                .Where(x => x.Successful)
                .ToList();

            if (!successfullEvents.Any())
                return;

            try
            {
                await PushEvents(chain, stream, successfullEvents, catchup, environment);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Couldn't push events from stream: {0}", stream);
            }
        }

        private async Task PushEvents(AppFunc chain, string stream, IEnumerable<DeSerializationResult> events, bool catchup, IDictionary<string, object> environment)
        {
            var requestEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                requestEnvironment[item.Key] = item.Value;

            var request = requestEnvironment.GetEventStoreRequest();

            request.Stream = stream;
            request.Events = events;
            request.IsCatchUp = catchup;
            request.OnException = (exception, evnt) => OnServiceError(stream, evnt.Data, evnt.Metadata, exception);

            await chain(requestEnvironment);
        }
    }
}