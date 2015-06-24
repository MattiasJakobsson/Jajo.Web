using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class ConfigureDiagnostics : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Diagnostics.Configured", environment =>
            {
                environment.RegisterSingleton(typeof(IManageDiagnosticsInformation), new ManageDiagnosticsInformationInMemory());
            }, "superglue.ContainerSetup");
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