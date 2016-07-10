using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.EventStore.Subscribers
{
    public class ConfigureSubscribers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStoreSubscribersSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(ISubscriberInstaller), typeof(DefaultSubscriberInstaller))
                    .Scan(typeof(ISubscribeTo<>)));

                environment.AlterSettings<SubscribersSettings>(x => x.FindPersistentSubscriptionNameUsing((serviceName, stream) => $"{serviceName}-{stream}"));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}