using System.Collections.Generic;
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
                environment.RegisterTransient(typeof(ISubscriberInstaller), typeof(DefaultSubscriberInstaller));

                environment.AlterSettings<SubscribersSettings>(x => x.FindPersistentSubscriptionNameUsing((serviceName, stream) => $"{serviceName}-{stream}"));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}