using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Consul;
using SuperGlue.Configuration;
using SuperGlue.Monitoring;

namespace SuperGlue.Discovery.Consul.Checks.Ttl
{
    public class SetupConsulTtlCheckConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ConsulTtlCheckSetup", environment =>
            {
                environment.AlterSettings<ConsulTtlCheckSettings>(x =>
                {
                    x.WithTtl(TimeSpan.FromSeconds(10)).WithRequestInterval(TimeSpan.FromSeconds(5));
                });

                return Task.CompletedTask;
            }, "superglue.ConsulSetup", configureAction: settings =>
            {
                var ttlSettings = settings.WithSettings<ConsulTtlCheckSettings>();

                settings
                    .WithSettings<ConsulServiceSettings>()
                    .WithCheck(new AgentServiceCheck
                    {
                        TTL = ttlSettings.Ttl
                    });

                settings
                    .WithSettings<HeartBeatSettings>()
                    .HeartBeatTo(new HandleConsulTtlCheck(), ttlSettings.RequestInterval);

                return Task.CompletedTask;
            });
        }
    }
}