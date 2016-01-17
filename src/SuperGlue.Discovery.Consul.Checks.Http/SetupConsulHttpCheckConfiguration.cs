using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using SuperGlue.Configuration;
using SuperGlue.Discovery.Consul.Checks.Http.Api;
using SuperGlue.Web.Routing;

namespace SuperGlue.Discovery.Consul.Checks.Http
{
    public class SetupConsulHttpCheckConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ConsulHttpCheckSetup", environment =>
            {
                environment.AlterSettings<ConsulHttpCheckSettings>(x =>
                {
                    x.WithInterval(TimeSpan.FromSeconds(10)).WithRoute("_healthcheck");
                });

                environment.AlterSettings<RouteSettings>(x =>
                {
                    x.UsePolicy(new HealthCheckEndpointRoutePolicy(environment.GetSettings<ConsulHttpCheckSettings>()));
                });

                var binding = environment.GetWebBindings().FirstOrDefault();

                if (!string.IsNullOrEmpty(binding))
                {
                    environment.AlterSettings<ConsulServiceSettings>(x =>
                    {
                        var url = new Uri(binding);

                        x
                            .WithAddress(url.DnsSafeHost)
                            .WithPort(url.Port)
                            .WithTag("web");
                    });
                }

                return Task.CompletedTask;
            }, "superglue.ConsulSetup", configureAction: settings =>
            {
                var httpSettings = settings.WithSettings<ConsulHttpCheckSettings>();

                var bindings = settings.Settings.GetWebBindings().ToList();

                if(!bindings.Any())
                    return Task.CompletedTask;

                var binding = new Uri(bindings.First());

                settings
                    .WithSettings<ConsulServiceSettings>()
                    .WithCheck(new AgentServiceCheck
                    {
                        Interval = httpSettings.Interval,
                        HTTP = new Uri(binding, httpSettings.CheckEndpointRoute).ToString()
                    });

                return Task.CompletedTask;
            });
        }
    }
}