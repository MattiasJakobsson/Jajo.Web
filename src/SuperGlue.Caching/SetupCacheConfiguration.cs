using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Caching
{
    public class SetupCacheConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.CacheSetup", environment =>
            {
                environment.RegisterTransient(typeof(ICache), typeof(InMemoryCache));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}