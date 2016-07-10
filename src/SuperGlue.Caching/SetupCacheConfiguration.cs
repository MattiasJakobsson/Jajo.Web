using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Caching
{
    public class SetupCacheConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.CacheSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(ICache), typeof(InMemoryCache)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}