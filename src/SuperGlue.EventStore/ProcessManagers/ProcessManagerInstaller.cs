using System;
using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using SuperGlue.EventStore.Data;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ProcessManagerInstaller : IProcessManagerInstaller
    {
        private readonly IEnumerable<IManageProcess> _processManagers;
        private readonly EventStoreConnectionString _eventStoreConnectionString;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILogger _logger;

        public ProcessManagerInstaller(IEnumerable<IManageProcess> processManagers, EventStoreConnectionString eventStoreConnectionString, IEventStoreConnection eventStoreConnection, ILogger logger)
        {
            _processManagers = processManagers;
            _eventStoreConnectionString = eventStoreConnectionString;
            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
        }

        public void InstallProjectionFor<TProcessManager>() where TProcessManager : IManageProcess
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

                projectionManager.CreateOrUpdateContinuousQueryAsync(name, query, credentials).Wait();
            }
        }

        public void InstallConsumerGroupFor<TProcessManager>(Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null) where TProcessManager : IManageProcess
        {
            var matchingProcessManagers = _processManagers.OfType<TProcessManager>().ToList();

            var settings = PersistentSubscriptionSettings.Create().WithNamedConsumerStrategy("PinnedPerPartitionKey");

            alterSettings = alterSettings ?? (x => x);

            settings = alterSettings(settings);

            foreach (var processManager in matchingProcessManagers)
            {
                try
                {
                    _eventStoreConnection
                        .CreatePersistentSubscriptionAsync(processManager.ProcessName, processManager.ProcessName, settings, _eventStoreConnectionString.GetUserCredentials())
                        .Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to create consumer group for process manager: {0}", processManager.ProcessName);
                }
            }
        }
    }
}