using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Superscribe.Engine;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public class ConventionalRoutingConfiguration
    {
        private readonly IEnumerable<IFilterEndpoints> _filterEndpoints;
        private readonly IEnumerable<IRoutePolicy> _policies;

        public ConventionalRoutingConfiguration(IEnumerable<IFilterEndpoints> filterEndpoints, IEnumerable<IRoutePolicy> policies)
        {
            _filterEndpoints = filterEndpoints;
            _policies = policies;
        }

        public IRouteEngine Configure(IEnumerable<Assembly> assemblies)
        {
            var possibleEndpoints = AppDomainAssemblyTypeScanner.GetMethodsInAssemblies(assemblies);

            possibleEndpoints = possibleEndpoints.Where(x => _filterEndpoints.All(y => y.IsValidEndpoint(x))).ToList();

            var define = RouteEngineFactory.Create();

            foreach (var endpoint in possibleEndpoints)
            {
                var matchingPolicy = _policies.FirstOrDefault(x => x.Matches(endpoint));

                if(matchingPolicy == null)
                    continue;

                var routeBuilder = new RouteBuilder();

                matchingPolicy.Build(endpoint, routeBuilder);

                routeBuilder.Build(define.Base, endpoint);
            }

            return define;
        }
    }
}