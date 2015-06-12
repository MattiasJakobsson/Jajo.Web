using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Caching
{
    public class SetupCacheConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.CacheSetup", environment =>
            {
                environment.RegisterTransient(typeof(ICache), typeof(InMemoryCache));
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