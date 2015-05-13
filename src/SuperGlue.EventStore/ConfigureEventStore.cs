using System;
using System.Collections.Generic;
using EventStore.ClientAPI;
using SuperGlue.Web;
using SuperGlue.Web.Configuration;

namespace SuperGlue.EventStore
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

                var registerSingleton = environment.Get<Action<Type, object>>("superglue.Container.RegisterSingleton");
                
                registerSingleton(typeof(IEventStoreConnection), connection);
                registerSingleton(typeof (EventStoreConnectionString), connectionString);
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }
    }
}