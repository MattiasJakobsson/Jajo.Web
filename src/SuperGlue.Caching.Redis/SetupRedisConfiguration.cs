using System.Collections.Generic;
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
                environment.AlterSettings<RedisCacheSettings>(x =>
                {
                    var connectionString = environment.Resolve<IApplicationConfiguration>().GetConnectionString("Redis.Cache");

                    if (string.IsNullOrEmpty(connectionString))
                        connectionString = "localhost:6379,abortConnect=false";

                    if (string.IsNullOrEmpty(x.ConnectionString))
                        x.UseConnectionString(connectionString);
                });

                environment.RegisterSingleton(typeof(ConnectionMultiplexer), (x, y) => ConnectionMultiplexer.Connect(y.GetSettings<RedisCacheSettings>().ConnectionString));

                return Task.CompletedTask;
            }, "superglue.CacheSetup");
        }
    }
}