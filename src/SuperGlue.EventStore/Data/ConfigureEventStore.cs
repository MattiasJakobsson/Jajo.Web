using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Data
{
    public class ConfigureEventStore : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Setup", environment =>
            {
                var connectionStringName = environment.Get<string>("eventstore.ConnectionStringName") ?? "EventStore";

                var connectionString = new EventStoreConnectionString(connectionStringName);

                var connection = connectionString.CreateConnection(x =>
                {
                    x.KeepRetrying();
                    x.KeepReconnecting();
                });

                connection.ConnectAsync();

                environment.RegisterSingleton(typeof(IEventStoreConnection), connection);
                environment.RegisterSingleton(typeof(EventStoreConnectionString), connectionString);
                environment.RegisterAll(typeof(IManageChanges));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}