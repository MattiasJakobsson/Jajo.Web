using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Subscribers
{
    public class ConfigureSubscribers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment => environment.RegisterAllClosing(typeof(ISubscribeTo<>)));
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}