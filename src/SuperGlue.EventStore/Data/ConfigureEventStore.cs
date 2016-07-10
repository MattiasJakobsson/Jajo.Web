using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;
using SuperGlue.EventStore.ConflictManagement;
using SuperGlue.EventStore.Timeouts;

namespace SuperGlue.EventStore.Data
{
    public class ConfigureEventStore : ISetupConfigurations
    {
        private static EventStoreConnectionString connectionString;
        private static IEventStoreConnection connection;

        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Setup", environment =>
            {
                environment.AlterSettings<EventStoreSettings>(x =>
                {
                    x.ModifySettings(y =>
                    {
                        y.KeepReconnecting();
                        y.KeepRetrying();
                        y.UseCustomLogger(new EventStoreLog(environment));
                    });

                    x.StoreCommands((env, command, id, causationId) => "commands");
                });

                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IEventStoreConnection), (y, z) => connection, RegistrationLifecycle.Singletone)
                    .Register(typeof(EventStoreConnectionString), (y, z) => connectionString, RegistrationLifecycle.Singletone)
                    .Register(typeof(IHandleEventSerialization), typeof(DefaultEventSerializer))
                    .Register(typeof(IRepository), typeof(DefaultRepository))
                    .Register(typeof(ICheckConflicts), typeof(DefaultConflictChecker))
                    .Register(typeof(IManageTimeOuts), typeof(DefaultTimeOutManager))
                    .Scan(typeof(IManageChanges))
                    .ScanOpenType(typeof(ICheckConflict<,>)));

                TimeOutManager.Configure(() => new StoreTimeoutsInMemory());

                return Task.CompletedTask;
            }, "superglue.LoggingSetup", environment =>
            {
                connection.Close();

                return Task.CompletedTask;
            }, configuration =>
            {
                var connectionInformation = configuration
                    .WithSettings<EventStoreSettings>()
                    .CreateConnection(configuration.Settings.Resolve<IApplicationConfiguration>());

                connection = connectionInformation.Item2;
                connectionString = connectionInformation.Item1;

                return connection.ConnectAsync();
            });
        }
    }
}