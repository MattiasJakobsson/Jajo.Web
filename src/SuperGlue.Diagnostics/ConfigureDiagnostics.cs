using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Diagnostics
{
    public class ConfigureDiagnostics : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.Diagnostics.Configured", environment =>
            {
                environment.RegisterSingleton(typeof(IManageDiagnosticsInformation), new ManageDiagnosticsInformationInMemory());
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }

        public void Configure(SettingsConfiguration configuration)
        {
            
        }
    }
}