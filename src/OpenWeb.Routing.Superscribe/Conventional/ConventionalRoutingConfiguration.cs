using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Superscribe.Engine;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public class ConventionalRoutingConfiguration
    {
        private readonly ICollection<IFilterEndpoints> _filterEndpoints = new Collection<IFilterEndpoints>();
        private readonly ICollection<IRoutePolicy> _policies = new Collection<IRoutePolicy>();

        private ConventionalRoutingConfiguration()
        {

        }

        public static ConventionalRoutingConfiguration New()
        {
            return new ConventionalRoutingConfiguration();
        }

        public ConventionalRoutingConfiguration UseEndpointFilterer(IFilterEndpoints filterEndpoints)
        {
            _filterEndpoints.Add(filterEndpoints);
            return this;
        }

        public ConventionalRoutingConfiguration UseRoutePolicy(IRoutePolicy routePolicy)
        {
            _policies.Add(routePolicy);
            return this;
        }

        public IRouteEngine Configure(IEnumerable<Assembly> assemblies, IDictionary<string, object> environmentSettings)
        {
            var possibleEndpoints = AppDomainAssemblyTypeScanner.GetMethodsInAssemblies(assemblies);

            possibleEndpoints = possibleEndpoints.Where(x => _filterEndpoints.All(y => y.IsValidEndpoint(x))).ToList();

            var define = RouteEngineFactory.Create();

            foreach (var endpoint in possibleEndpoints)
            {
                var matchingPolicy = _policies.FirstOrDefault(x => x.Matches(endpoint));

                if (matchingPolicy == null)
                    continue;

                var routeBuilder = new RouteBuilder();

                matchingPolicy.Build(endpoint, routeBuilder);

                routeBuilder.Build(define.Base, endpoint);
            }

            environmentSettings["openweb.Superscribe.Engine"] = define;

            return define;
        }
    }
}