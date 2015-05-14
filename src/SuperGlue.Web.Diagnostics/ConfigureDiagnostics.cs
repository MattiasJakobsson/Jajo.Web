using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Diagnostics
{
    public class ConfigureDiagnostics : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.Diagnostics.Configured", 
                environment => environment.Get<Action<Type, object>>("superglue.Container.RegisterSingleton")(typeof(IManageDiagnosticsInformation), new ManageDiagnosticsInformationInMemory()),
                "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}