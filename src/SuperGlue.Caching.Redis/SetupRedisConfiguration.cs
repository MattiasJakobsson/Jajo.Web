using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using StackExchange.Redis;
using SuperGlue.Configuration;

namespace SuperGlue.Caching.Redis
{
    public class SetupRedisConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Cache.RedisSetup", environment =>
            {
                environment.RegisterTransient(typeof(ICache), typeof(RedisCacheProvider));
                environment.RegisterTransient(typeof(IRedisDataSerializer), typeof(DefaultRedisDataSerializer));
                //HACK:Hard coded connectionstring name
                environment.RegisterSingleton(typeof(ConnectionMultiplexer), (x, y) => ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["Redis.Cache"].ConnectionString));

                return Task.CompletedTask;
            }, "superglue.CacheSetup");
        }
    }
}