using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.ProcessManagers
{
    public class ConfigureProcessManagers : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.ProcessManagers.Configured", environment => environment.RegisterAll(typeof(IManageProcess)), "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}