using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Subscribers
{
    public class ConfigureSubscribers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStoreSubscribersSetup", environment =>
            {
                environment.RegisterAllClosing(typeof (ISubscribeTo<>));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup", configureAction: x =>
            {
                var settings = x.WithSettings<SubscribersSettings>();

                settings
                    .FindPersistentSubscriptionNameUsing((serviceName, stream) => string.Format("{0}-{1}", serviceName, stream))
                    .ServiceName(ConfigurationManager.AppSettings["Service.Name"]);

                var streams = (ConfigurationManager.AppSettings["EventStore.Streams"] ?? "").Split(';').Where(y => !string.IsNullOrWhiteSpace(y)).ToList();

                foreach (var stream in streams)
                    settings.SubscribeToStream(stream, ConfigurationManager.AppSettings[string.Format("EventStore.Streams.{0}.LiveOnly", stream)] == "true");

                return Task.CompletedTask;
            });
        }
    }
}