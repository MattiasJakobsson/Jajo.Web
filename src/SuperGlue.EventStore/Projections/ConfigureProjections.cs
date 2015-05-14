using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Projections
{
    public class ConfigureProjections : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Projections.Configured", environment => environment.RegisterAll(typeof(IEventStoreProjection)), "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}