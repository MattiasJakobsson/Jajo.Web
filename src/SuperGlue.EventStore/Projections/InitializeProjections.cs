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
using SuperGlue.Logging;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeProjections : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEnumerable<IEventStoreProjection> _projections;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private readonly ILog _log;
        private static bool running;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions = new Dictionary<string, ProjectionSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeProjections(IHandleEventSerialization eventSerialization, IEnumerable<IEventStoreProjection> projections, IWriteToErrorStream writeToErrorStream,
            IEventStoreConnection eventStoreConnection, ILog log)
        {
            _eventSerialization = eventSerialization;
            _projections = projections;
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

        public string Chain { get { return "chains.Projections"; } }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            _log.Info("Starting projections for environment: {0}", environment);

            running = true;

            foreach (var projection in _projections)
                await SubscribeProjection(projection, chain, settings);
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            _log.Info("Shutting down projections");

            running = false;

            foreach (var subscription in _projectionSubscriptions)
                subscription.Value.Close();

            _projectionSubscriptions.Clear();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            _log.Info("Getting default chain for projections for environment: {0}", environment);
            return buildApp.Use<ExecuteProjection>().Use<SetLastEvent>().Build();
        }

        protected virtual void OnProjectionError(IEventStoreProjection projection, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ProjectionFailed(projection.ProjectionName, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
            _log.Error(exception, "Error while processing event of type: {0} for projection: {1}", message != null ? message.GetType().FullName : "Unknown", projection.ProjectionName);
        }

        private async Task SubscribeProjection(IEventStoreProjection currentEventStoreProjection, AppFunc chain, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            _log.Info("Subscribing projection: {0}", currentEventStoreProjection.ProjectionName);

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

                    var eventStoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(currentEventStoreProjection.ProjectionName, await eventNumberManager.GetLastEvent(currentEventStoreProjection.ProjectionName, environment), true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        subscriptionDropped: async (subscription, reason, exception) => await SubscriptionDropped(chain, currentEventStoreProjection, reason, exception, environment));

                    _projectionSubscriptions[currentEventStoreProjection.ProjectionName] = new ProjectionSubscription(messageSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    _log.Error(ex, "Couldn't subscribe projection: {0}. Retrying in 500 ms.", currentEventStoreProjection.ProjectionName);

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }
        }

        private async Task SubscriptionDropped(AppFunc chain, IEventStoreProjection projection, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            _log.Warn(exception, "Subscription dropped for projection: {0}. Reason: {1}. Retrying...", projection.ProjectionName, reason);

            await SubscribeProjection(projection, chain, environment);
        }

        private async Task PushEventsToProjections(AppFunc chain, IEventStoreProjection projection, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var failedEvents = eventsList.Where(x => !x.Successful);
            foreach (var evnt in failedEvents)
                OnProjectionError(projection, evnt.Data, evnt.Metadata, evnt.Error);

            var successfullEvents = eventsList
                .Where(x => x.Successful)
                .ToList();

            if (!successfullEvents.Any())
                return;

            try
            {
                await PushEvents(chain, projection, successfullEvents, environment);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Couldn't push events to projection: {0}", projection.ProjectionName);
            }
        }

        private async Task PushEvents(AppFunc chain, IEventStoreProjection projection, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var requestEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                requestEnvironment[item.Key] = item.Value;

            var request = requestEnvironment.GetEventStoreRequest();

            request.Projection = projection;
            request.Events = events;
            request.OnException = (exception, evnt) => OnProjectionError(projection, evnt.Data, evnt.Metadata, exception);

            await chain(requestEnvironment);
        }
    }
}