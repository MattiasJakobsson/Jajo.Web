using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Data;
using SuperGlue.UnitOfWork;

namespace SuperGlue.EventStore.ProcessManagers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeProcessManagers : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEnumerable<IManageProcess> _processManagers;
        private readonly IEventStoreConnection _eventStoreConnection;
        private static bool running;
        private readonly IDictionary<string, ProcessManagerSubscription> _processManagerSubscriptions = new Dictionary<string, ProcessManagerSubscription>();

        public InitializeProcessManagers(IHandleEventSerialization eventSerialization, IEnumerable<IManageProcess> processManagers,
            IEventStoreConnection eventStoreConnection)
        {
            _eventSerialization = eventSerialization;
            _processManagers = processManagers;
            _eventStoreConnection = eventStoreConnection;
        }

        public string Chain => "chains.ProcessManagers";

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Starting processmanagers for environment: {0}", LogLevel.Debug, environment);

            running = true;

            foreach (var processManager in _processManagers)
                await SubscribeProcessManager(chain, processManager, settings).ConfigureAwait(false);
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            settings.Log("Shutting down processmanagers", LogLevel.Debug);

            running = false;

            foreach (var subscription in _processManagerSubscriptions)
                subscription.Value.Close();

            _processManagerSubscriptions.Clear();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Building default chain for processmanagers for environment: {0}", LogLevel.Debug, environment);
            return buildApp.Use<HandleUnitOfWork>(new HandleUnitOfWorkOptions(true)).Use<ExecuteProcessManager>().Build();
        }

        public NodeTypeRequirements SetupRequirements(NodeTypeRequirements nodeTypeRequirements)
        {
            return nodeTypeRequirements;
        }

        private async Task SubscribeProcessManager(AppFunc chain, IManageProcess currentProcessManager, IDictionary<string, object> environment)
        {
            environment.Log("Subscribing processmanager: {0}", LogLevel.Debug, currentProcessManager.ProcessName);

            while (true)
            {
                if (!running)
                    return;

                if (_processManagerSubscriptions.ContainsKey(currentProcessManager.ProcessName))
                {
                    _processManagerSubscriptions[currentProcessManager.ProcessName].Close();
                    _processManagerSubscriptions.Remove(currentProcessManager.ProcessName);
                }

                try
                {
                    var eventStoreSubscription = _eventStoreConnection.ConnectToPersistentSubscription(currentProcessManager.ProcessName, currentProcessManager.ProcessName,
                        async (subscription, evnt) => await PushEventToProcessManager(chain, currentProcessManager, _eventSerialization.DeSerialize(evnt), environment, subscription).ConfigureAwait(false),
                        async (subscription, reason, exception) => await SubscriptionDropped(chain, currentProcessManager, reason, exception, environment).ConfigureAwait(false),
                        autoAck: false);

                    _processManagerSubscriptions[currentProcessManager.ProcessName] = new ProcessManagerSubscription(eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    environment.Log(ex, "Couldn't subscribe processmanager: {0}. Retrying in 5 seconds.", LogLevel.Error, currentProcessManager.ProcessName);

                    await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
            }
        }

        private Task SubscriptionDropped(AppFunc chain, IManageProcess processManager, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return Task.CompletedTask;

            environment.Log(exception, "Subscription dropped for processmanager: {0}. Reason: {1}. Retrying...", LogLevel.Warn, processManager.ProcessName, reason);

            if (reason != SubscriptionDropReason.UserInitiated)
                return SubscribeProcessManager(chain, processManager, environment);

            return Task.CompletedTask;
        }

        private static async Task PushEventToProcessManager(AppFunc chain, IManageProcess processManager, DeSerializationResult evnt, IDictionary<string, object> environment, EventStorePersistentSubscriptionBase subscription)
        {
            if (!running)
                return;

            if (!evnt.Successful)
            {
                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, evnt.Error.Message);
                return;
            }

            try
            {
                var requestEnvironment = new Dictionary<string, object>();
                foreach (var item in environment)
                    requestEnvironment[item.Key] = item.Value;

                var request = requestEnvironment.GetEventStoreRequest();

                request.ProcessManager = processManager;
                request.Event = evnt;

                var correlationId = evnt.Metadata.Get(DefaultRepository.CorrelationIdKey, Guid.NewGuid().ToString());

                using (requestEnvironment.OpenCorrelationContext(correlationId))
                using (requestEnvironment.OpenCausationContext(evnt.EventId.ToString()))
                {
                    await chain(requestEnvironment).ConfigureAwait(false);
                }

                subscription.Acknowledge(evnt.OriginalEvent);
            }
            catch (Exception ex)
            {
                environment.Log(ex, "Couldn't push event to processmanager: {0}", LogLevel.Error, processManager.ProcessName);

                subscription.Fail(evnt.OriginalEvent, PersistentSubscriptionNakEventAction.Unknown, ex.Message);
            }
        }
    }
}