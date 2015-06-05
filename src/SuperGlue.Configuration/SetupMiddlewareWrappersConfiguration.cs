using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SetupMiddlewareWrappersConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.MiddlewareWrappersSetup", environment =>
                {
                    environment.RegisterAllClosing(typeof(IWrapMiddleware<>));
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