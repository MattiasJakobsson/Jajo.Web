using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Caching.Redis
{
    public class SetupRedisConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Cache.RedisSetup", environment =>
            {
                environment.AlterSettings<RedisCacheSettings>(x =>
                {
                    var connectionString = environment.Resolve<IApplicationConfiguration>().GetConnectionString("Redis.Cache");

                    if (string.IsNullOrEmpty(connectionString))
                        connectionString = "localhost:6379,abortConnect=false";

                    if (string.IsNullOrEmpty(x.ConnectionString))
                        x.UseConnectionString(connectionString);
                });

                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(ICache), typeof(RedisCacheProvider))
                    .Register(typeof(IRedisDataSerializer), typeof(DefaultRedisDataSerializer))
                    .Register(typeof(ConnectionMultiplexer), (y, z) => ConnectionMultiplexer.Connect(environment.GetSettings<RedisCacheSettings>().ConnectionString), RegistrationLifecycle.Singletone));

                return Task.CompletedTask;
            }, "superglue.CacheSetup");
        }
    }
}