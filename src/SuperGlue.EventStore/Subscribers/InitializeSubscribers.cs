using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Messages;

namespace SuperGlue.EventStore.Subscribers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeSubscribers : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IFindPartitionKey _findPartitionKey;
        private static bool running;
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions = new Dictionary<string, IServiceSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeSubscribers(IHandleEventSerialization eventSerialization, IWriteToErrorStream writeToErrorStream, IEventStoreConnection eventStoreConnection, IFindPartitionKey findPartitionKey)
        {
            _eventSerialization = eventSerialization;
            _writeToErrorStream = writeToErrorStream;
            _eventStoreConnection = eventStoreConnection;
            _findPartitionKey = findPartitionKey;

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

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            return Task.Factory.StartNew(() =>
            {
                running = true;

                var streams = ConfigurationManager.AppSettings["EventStore.Streams"].Split(';').ToList();
                var name = ConfigurationManager.AppSettings["Service.Name"];

                foreach (var stream in streams)
                    SubscribeService(chain, name, stream, settings);
            });
        }

        public Task ShutDown()
        {
            return Task.Factory.StartNew(() =>
            {
                running = false;

                foreach (var subscription in _serviceSubscriptions)
                    subscription.Value.Close();

                _serviceSubscriptions.Clear();
            });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return buildApp.Use<ExecuteSubscribers>().Build();
        }

        protected virtual void OnServiceError(string service, string stream, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ServiceEventProcessingFailed(service, stream, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
        }

        private void SubscribeService(AppFunc chain, string name, string stream, IDictionary<string, object> environment)
        {
            var liveOnlySubscriptions = ConfigurationManager.AppSettings["Service.Subscription.LiveOnly"] == "true";
            if (!running)
                return;

            while (true)
            {
                try
                {
                    SubscribeService(chain, name, stream, liveOnlySubscriptions, environment);
                    //TODO:Log

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    //TODO:Log
                }
            }
        }

        private void SubscribeService(AppFunc chain, string name, string stream, bool liveOnlySubscriptions, IDictionary<string, object> environment)
        {
            var subscriptionKey = string.Format("{0}-{1}", name, stream);

            if (_serviceSubscriptions.ContainsKey(subscriptionKey))
            {
                _serviceSubscriptions[subscriptionKey].Close();
                _serviceSubscriptions.Remove(subscriptionKey);
            }

                var messageProcessor = new MessageProcessor();

                var messageSubscription = Observable
                            .FromEvent<DeSerializationResult>(x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                            .Buffer(TimeSpan.FromSeconds(_dispatchWaitSeconds), _numberOfEventsPerBatch)
                            .Subscribe(async x => await PushEventsToService(chain, name, stream, x, !liveOnlySubscriptions, environment));

                if (liveOnlySubscriptions)
                {
                    var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamAsync(stream, true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        (subscription, reason, exception) => SubscriptionDropped(chain, name, stream, true, reason, exception, environment)).Result;

                    _serviceSubscriptions[subscriptionKey] = new LiveOnlyServiceSubscription(messageSubscription, eventstoreSubscription);
                }
                else
                {
                    var manageStreamEventNumbers = environment.Resolve<IManageEventNumbersForSubscriber>();

                    var lastEvent = manageStreamEventNumbers.GetLastEvent(name, stream);

                    var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(stream, lastEvent, true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        subscriptionDropped:
                            (subscription, reason, exception) => SubscriptionDropped(chain, name, stream, false, reason, exception, environment));

                    _serviceSubscriptions[subscriptionKey] = new CatchUpServiceSubscription(messageSubscription, eventstoreSubscription);
                }
        }

        private void SubscriptionDropped(AppFunc chain, string name, string stream, bool liveOnlySubscriptions, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            //TODO:Log

            SubscribeService(chain, name, stream, liveOnlySubscriptions, environment);
        }

        private async Task PushEventsToService(AppFunc chain, string name, string stream, IEnumerable<DeSerializationResult> events, bool catchup, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var failedEvents = eventsList.Where(x => !x.Successful);
            foreach (var evnt in failedEvents)
                OnServiceError(name, stream, evnt.Data, evnt.Metadata, evnt.Error);

            var groupedEvents = eventsList
                .Where(x => x.Successful)
                .GroupBy(x => _findPartitionKey.FindFrom(x.Metadata));

            foreach (var groupedEvent in groupedEvents)
            {
                try
                {
                    await PushEvents(chain, name, stream, groupedEvent.Select(x => x), groupedEvent.Key, catchup, environment);
                }
                catch (Exception ex)
                {
                    //TODO:Log
                }
            }
        }

        private async Task PushEvents(AppFunc chain, string service, string stream, IEnumerable<DeSerializationResult> events, string partitionKey, bool catchup, IDictionary<string, object> environment)
        {
            var requestEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                requestEnvironment[item.Key] = item.Value;

            var request = requestEnvironment.GetEventStoreRequest();

            request.Service = service;
            request.Stream = stream;
            request.Events = events;
            request.PartitionKey = partitionKey;
            request.IsCatchUp = catchup;
            request.OnException = (exception, evnt) => OnServiceError(service, stream, evnt.Data, evnt.Metadata, exception);

            await chain(requestEnvironment);
        }
    }
}