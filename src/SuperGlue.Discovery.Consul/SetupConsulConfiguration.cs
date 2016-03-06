using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Discovery.Consul
{
    public class SetupConsulConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ConsulSetup", environment =>
            {
                environment.AlterSettings<ConsulServiceSettings>(x =>
                {
                    var settings = x
                        .WithId(Guid.NewGuid().ToString())
                        .WithName(environment.GetApplicationName().ToLower());

                    foreach (var tag in environment.GetTags())
                        settings.WithTag(tag);
                });

                return Task.CompletedTask;
            });
        }
    }
}