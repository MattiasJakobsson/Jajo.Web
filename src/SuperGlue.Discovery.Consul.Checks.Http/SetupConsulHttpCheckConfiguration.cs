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
                    x.WithInterval(TimeSpan.FromMinutes(1)).WithRoute(new HealthCheckInput(), "_healthcheck");
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

                        var address = url.DnsSafeHost;

                        if (address == "localhost")
                            address = null;

                        x.WithAddress(address)
                            .WithPort(url.Port);
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
                        HTTP = new Uri(binding, settings.Settings.RouteTo(httpSettings.CheckEndpoint.Input)).ToString(),
                        DeregisterCriticalServiceAfter = httpSettings.DeregisterCriticalServiceAfter,
	                    Status = HealthStatus.Passing
                    });

                return Task.CompletedTask;
            });
        }
    }
}