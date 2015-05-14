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

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeProjections : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEnumerable<IEventStoreProjection> _projections;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private readonly IFindPartitionKey _findPartitionKey;
        private static bool running;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions = new Dictionary<string, ProjectionSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeProjections(IHandleEventSerialization eventSerialization, IEnumerable<IEventStoreProjection> projections, IWriteToErrorStream writeToErrorStream,
            IEventStoreConnection eventStoreConnection, IFindPartitionKey findPartitionKey)
        {
            _eventSerialization = eventSerialization;
            _projections = projections;
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

        public void Start(IDictionary<string, AppFunc> chains, IDictionary<string, object> environment)
        {
            if(!chains.ContainsKey("chains.Projections"))
                return;

            var chain = chains["chains.Projections"];

            running = true;

            foreach (var projection in _projections)
                SubscribeProjection(projection, chain, environment);
        }

        public void ShutDown()
        {
            running = false;

            foreach (var subscription in _projectionSubscriptions)
                subscription.Value.Close();

            _projectionSubscriptions.Clear();
        }

        protected virtual void OnProjectionError(IEventStoreProjection projection, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ProjectionFailed(projection.ProjectionName, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
        }

        private void SubscribeProjection(IEventStoreProjection currentEventStoreProjection, AppFunc chain, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            while (true)
            {
                if (_projectionSubscriptions.ContainsKey(currentEventStoreProjection.ProjectionName))
                    _projectionSubscriptions.Remove(currentEventStoreProjection.ProjectionName);

                try
                {
                    var eventNumberManager = environment.Resolve<IManageEventNumbersForProjections>();

                    var messageProcessor = new MessageProcessor();
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(_dispatchWaitSeconds), _numberOfEventsPerBatch)
                        .Subscribe(async x => await PushEventsToProjections(chain, currentEventStoreProjection, x, environment));

                    var eventStoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(currentEventStoreProjection.ProjectionName, eventNumberManager.GetLastEvent(currentEventStoreProjection.ProjectionName), true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        subscriptionDropped: (subscription, reason, exception) => SubscriptionDropped(chain, currentEventStoreProjection, reason, exception, environment));

                    _projectionSubscriptions[currentEventStoreProjection.ProjectionName] = new ProjectionSubscription(messageSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    //TODO:Log

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
        }

        private void SubscriptionDropped(AppFunc chain, IEventStoreProjection projection, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            //TODO:Log

            SubscribeProjection(projection, chain, environment);
        }

        private async Task PushEventsToProjections(AppFunc chain, IEventStoreProjection projection, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var failedEvents = eventsList.Where(x => !x.Successful);
            foreach (var evnt in failedEvents)
                OnProjectionError(projection, evnt.Data, evnt.Metadata, evnt.Error);

            var groupedEvents = eventsList
                .Where(x => x.Successful)
                .GroupBy(x => _findPartitionKey.FindFrom(x.Metadata));

            foreach (var groupedEvent in groupedEvents)
            {
                try
                {
                    await PushEvents(chain, projection, groupedEvent.Select(x => x), groupedEvent.Key, environment);
                }
                catch (Exception ex)
                {
                    //TODO:Log
                }
            }
        }

        private async Task PushEvents(AppFunc chain, IEventStoreProjection projection, IEnumerable<DeSerializationResult> events, string partitionKey, IDictionary<string, object> environment)
        {
            var requestEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                requestEnvironment[item.Key] = item.Value;

            requestEnvironment["superglue.EventStore.Projection"] = projection;
            requestEnvironment["superglue.EventStore.Events"] = events;
            requestEnvironment["superglue.EventStore.PartitionKey"] = partitionKey;
            requestEnvironment["superglue.EventStore.OnException"] = (Action<Exception, DeSerializationResult>)((exception, evnt) => OnProjectionError(projection, evnt.Data, evnt.Metadata, exception));

            await chain(requestEnvironment);
        }
    }
}