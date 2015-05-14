using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SetupApplicationConfigurations : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.ApplicationsConfigured", environment => environment.RegisterAll(typeof (IStartApplication)), "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}