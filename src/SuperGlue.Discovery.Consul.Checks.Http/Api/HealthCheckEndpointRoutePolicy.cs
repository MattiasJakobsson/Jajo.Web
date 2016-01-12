using System;
using System.Collections.Generic;
using SuperGlue.Web.Routing;

namespace SuperGlue.Discovery.Consul.Checks.Http.Api
{
    public class HealthCheckEndpointRoutePolicy : IRoutePolicy
    {
        private readonly ConsulHttpCheckSettings _settings;

        public HealthCheckEndpointRoutePolicy(ConsulHttpCheckSettings settings)
        {
            _settings = settings;
        }

        public IEnumerable<EndpointInformation> Build()
        {
            yield return new EndpointInformation(typeof(HealthCheckEndpoint).GetMethod("Check"), new List<IUrlPart>
            {
                new StaticUrlPart(_settings.CheckEndpointRoute)
            }, new Dictionary<Type, Func<object, IDictionary<string, object>>>(), new[] { "GET" });
        }
    }
}