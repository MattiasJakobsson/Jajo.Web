using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class SetupApplicationConfigurations : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.ApplicationsConfigured", environment => environment.RegisterAll(typeof (IStartApplication)), "superglue.ContainerSetup");
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