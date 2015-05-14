using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.EventStore.Messages;

namespace SuperGlue.EventStore.Subscribers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeSubscribers : IInitializeSubscribers
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IFindPartitionKey _findPartitionKey;
        private readonly IDictionary<string, object> _environment;
        private static bool running;
        private readonly IDictionary<string, IServiceSubscription> _serviceSubscriptions = new Dictionary<string, IServiceSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeSubscribers(IHandleEventSerialization eventSerialization, IWriteToErrorStream writeToErrorStream, IEventStoreConnection eventStoreConnection, IFindPartitionKey findPartitionKey, IDictionary<string, object> environment)
        {
            _eventSerialization = eventSerialization;
            _writeToErrorStream = writeToErrorStream;
            _eventStoreConnection = eventStoreConnection;
            _findPartitionKey = findPartitionKey;
            _environment = environment;

            _dispatchWaitSeconds = 1;

            int dispatchWaitSeconds;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.WaitSeconds"] ?? "", out dispatchWaitSeconds))
                _dispatchWaitSeconds = dispatchWaitSeconds;

            _numberOfEventsPerBatch = 256;

            int numberOfEventsPerBatch;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.NumberOfEventsPerBatch"] ?? "", out numberOfEventsPerBatch))
                _numberOfEventsPerBatch = numberOfEventsPerBatch;
        }

        public void Initialize(AppFunc chain, string name)
        {
            running = true;
            var streams = ConfigurationManager.AppSettings["EventStore.Streams"].Split(';').ToList();

            foreach (var stream in streams)
                SubscribeService(chain, name, stream);
        }

        public void Stop()
        {
            running = false;

            foreach (var subscription in _serviceSubscriptions)
                subscription.Value.Close();

            _serviceSubscriptions.Clear();
        }

        protected virtual void OnServiceError(string service, string stream, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ServiceEventProcessingFailed(service, stream, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
        }

        private void SubscribeService(AppFunc chain, string name, string stream)
        {
            var liveOnlySubscriptions = ConfigurationManager.AppSettings["Service.Subscription.LiveOnly"] == "true";
            if (!running)
                return;

            while (true)
            {
                try
                {
                    SubscribeService(chain, name, stream, liveOnlySubscriptions);
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

        private void SubscribeService(AppFunc chain, string name, string stream, bool liveOnlySubscriptions)
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
                            .Subscribe(x => PushEventsToService(chain, name, stream, x, !liveOnlySubscriptions));

                if (liveOnlySubscriptions)
                {
                    var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamAsync(stream, true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        (subscription, reason, exception) => SubscriptionDropped(chain, name, stream, true, reason, exception)).Result;

                    _serviceSubscriptions[subscriptionKey] = new LiveOnlyServiceSubscription(messageSubscription, eventstoreSubscription);
                }
                else
                {
                    var manageStreamEventNumbers = _environment.Resolve<IManageEventNumbersForSubscriber>();

                    var lastEvent = manageStreamEventNumbers.GetLastEvent(name, stream);

                    var eventstoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(stream, lastEvent, true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        subscriptionDropped:
                            (subscription, reason, exception) => SubscriptionDropped(chain, name, stream, false, reason, exception));

                    _serviceSubscriptions[subscriptionKey] = new CatchUpServiceSubscription(messageSubscription, eventstoreSubscription);
                }
        }

        private void SubscriptionDropped(AppFunc chain, string name, string stream, bool liveOnlySubscriptions, SubscriptionDropReason reason, Exception exception)
        {
            if (!running)
                return;

            //TODO:Log

            SubscribeService(chain, name, stream, liveOnlySubscriptions);
        }

        private void PushEventsToService(AppFunc chain, string name, string stream, IEnumerable<DeSerializationResult> events, bool catchup)
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
                    PushEvents(chain, name, stream, groupedEvent.Select(x => x), groupedEvent.Key, catchup);
                }
                catch (Exception ex)
                {
                    //TODO:Log
                }
            }
        }

        private void PushEvents(AppFunc chain, string service, string stream, IEnumerable<DeSerializationResult> events, string partitionKey, bool catchup)
        {
            var environment = new Dictionary<string, object>();
            foreach (var item in _environment)
                environment[item.Key] = item.Value;

            environment["superglue.EventStore.Service"] = service;
            environment["superglue.EventStore.Stream"] = stream;
            environment["superglue.EventStore.Events"] = events;
            environment["superglue.EventStore.PartitionKey"] = partitionKey;
            environment["superglue.EventStore.IsCatchUp"] = catchup;
            environment["superglue.EventStore.OnException"] = (Action<Exception, DeSerializationResult>)((exception, evnt) => OnServiceError(service, stream, evnt.Data, evnt.Metadata, exception));

            chain(environment);
        }
    }
}