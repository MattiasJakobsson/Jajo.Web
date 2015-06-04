using System.Collections.Generic;
using EventStore.ClientAPI;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Data
{
    public class ConfigureEventStore : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
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
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }

        public void Configure(SettingsConfiguration configuration)
        {
            
        }
    }
}