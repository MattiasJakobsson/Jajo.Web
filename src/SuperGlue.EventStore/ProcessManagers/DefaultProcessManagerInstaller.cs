using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class DefaultProcessManagerInstaller : IProcessManagerInstaller
    {
        private readonly IEnumerable<IManageProcess> _processManagers;
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IDictionary<string, object> _environment;

        public DefaultProcessManagerInstaller(IEnumerable<IManageProcess> processManagers, EventStoreConnectionString eventStoreConnectionString, IEventStoreConnection eventStoreConnection, IDictionary<string, object> environment)
        {
            _processManagers = processManagers;
            _eventStoreConnectionString = eventStoreConnectionString;
            _eventStoreConnection = eventStoreConnection;
            _environment = environment;
        }

        public async Task InstallProjectionFor<TProcessManager>() where TProcessManager : IManageProcess
        {
            var matchingProcessManagers = _processManagers.OfType<TProcessManager>().ToList();

            var projectionBuilder = new ProjectionBuilder();
            var projectionManager = _eventStoreConnectionString.CreateProjectionsManager();
            var credentials = _eventStoreConnectionString.GetUserCredentials();

            foreach (var processManager in matchingProcessManagers)
            {
                var name = string.Format("project-to-{0}", processManager.ProcessName);
                var query = projectionBuilder.BuildStreamProjection(processManager.GetStreamsToProcess(),
                    processManager.ProcessName,
                    processManager.GetEventMappings().Select(x => new ProjectionBuilder.EventMap(x.Key.Name, x.Value)));

                await projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials);
            }
        }

        public async Task InstallConsumerGroupFor<TProcessManager>(Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null) where TProcessManager : IManageProcess
        {
            var matchingProcessManagers = _processManagers.OfType<TProcessManager>().ToList();

            var settings = PersistentSubscriptionSettings.Create().WithNamedConsumerStrategy("PinnedPerPartitionKey").ResolveLinkTos();

            alterSettings = alterSettings ?? (x => x);

            settings = alterSettings(settings);

            foreach (var processManager in matchingProcessManagers)
            {
                try
                {
                    await _eventStoreConnection
                        .CreatePersistentSubscriptionAsync(processManager.ProcessName, processManager.ProcessName, settings, _eventStoreConnectionString.GetUserCredentials());
                }
                catch (Exception ex)
                {
                    _environment.Log(ex, "Failed to create consumer group for process manager: {0}", LogLevel.Error, processManager.ProcessName);
                }
            }
        }
    }
}