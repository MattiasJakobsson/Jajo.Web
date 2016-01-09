using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.ExceptionManagement;
using SuperGlue.UnitOfWork;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeProjections : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEnumerable<IEventStoreProjection> _projections;
        private readonly IEventStoreConnection _eventStoreConnection;
        private static bool running;
        private readonly IDictionary<string, ProjectionSubscription> _projectionSubscriptions = new Dictionary<string, ProjectionSubscription>();

        public InitializeProjections(IHandleEventSerialization eventSerialization, IEnumerable<IEventStoreProjection> projections, IEventStoreConnection eventStoreConnection)
        {
            _eventSerialization = eventSerialization;
            _projections = projections;
            _eventStoreConnection = eventStoreConnection;
        }

        public string Chain { get { return "chains.Projections"; } }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Starting projections for environment: {0}", LogLevel.Debug, environment);

            running = true;

            foreach (var projection in _projections)
                await SubscribeProjection(projection, chain, settings);
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            settings.Log("Shutting down projections", LogLevel.Debug);

            running = false;

            foreach (var subscription in _projectionSubscriptions)
                subscription.Value.Close();

            _projectionSubscriptions.Clear();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Getting default chain for projections for environment: {0}", LogLevel.Debug, environment);

            return buildApp
                .Use<Retry>(5, TimeSpan.FromSeconds(1), "projection")
                .Use<HandleUnitOfWork>(true)
                .Use<ExecuteProjection>()
                .Use<SetLastEvent>()
                .Build();
        }

        private async Task SubscribeProjection(IEventStoreProjection currentEventStoreProjection, AppFunc chain, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            environment.Log("Subscribing projection: {0}", LogLevel.Debug, currentEventStoreProjection.ProjectionName);

            var bufferSettings = environment.GetSettings<ProjectionSettings>().GetBufferSettings();

            while (true)
            {
                if (_projectionSubscriptions.ContainsKey(currentEventStoreProjection.ProjectionName))
                {
                    _projectionSubscriptions[currentEventStoreProjection.ProjectionName].Close();
                    _projectionSubscriptions.Remove(currentEventStoreProjection.ProjectionName);
                }

                try
                {
                    var eventNumberManager = environment.Resolve<IManageEventNumbersForProjections>();

                    var messageProcessor = new MessageProcessor();
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(bufferSettings.Seconds), bufferSettings.NumberOfEvents)
                        .Subscribe(async x => await PushEventsToProjections(chain, currentEventStoreProjection, x, environment));

                    var eventStoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(currentEventStoreProjection.ProjectionName, await eventNumberManager.GetLastEvent(currentEventStoreProjection.ProjectionName, environment), true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt)),
                        subscriptionDropped: async (subscription, reason, exception) => await SubscriptionDropped(chain, currentEventStoreProjection, reason, exception, environment));

                    _projectionSubscriptions[currentEventStoreProjection.ProjectionName] = new ProjectionSubscription(messageSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    environment.Log(ex, "Couldn't subscribe projection: {0}. Retrying in 5 seconds.", LogLevel.Warn, currentEventStoreProjection.ProjectionName);

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }

        private void StopProjection(IEventStoreProjection projection)
        {
            if (!_projectionSubscriptions.ContainsKey(projection.ProjectionName))
                return;

            _projectionSubscriptions[projection.ProjectionName].Close();
            _projectionSubscriptions.Remove(projection.ProjectionName);
        }

        private async Task SubscriptionDropped(AppFunc chain, IEventStoreProjection projection, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            environment.Log(exception, "Subscription dropped for projection: {0}. Reason: {1}. Retrying...", LogLevel.Warn, projection.ProjectionName, reason);

            if (reason != SubscriptionDropReason.UserInitiated)
                await SubscribeProjection(projection, chain, environment);
        }

        private async Task PushEventsToProjections(AppFunc chain, IEventStoreProjection projection, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var successfullEvents = eventsList
                .Where(x => x.Successful)
                .ToList();

            if (!successfullEvents.Any())
                return;

            try
            {
                var requestEnvironment = new Dictionary<string, object>();
                foreach (var item in environment)
                    requestEnvironment[item.Key] = item.Value;

                var request = requestEnvironment.GetEventStoreRequest();

                request.Projection = projection;
                request.Events = eventsList;

                await chain(requestEnvironment);
            }
            catch (Exception ex)
            {
                environment.Log(ex, "Couldn't push events to projection: {0}", LogLevel.Error, projection.ProjectionName);

                StopProjection(projection);

                await environment.Notifications().Error("projections", string.Format("Projection: {0} failed!", projection.ProjectionName), ex);
            }
        }
    }
}