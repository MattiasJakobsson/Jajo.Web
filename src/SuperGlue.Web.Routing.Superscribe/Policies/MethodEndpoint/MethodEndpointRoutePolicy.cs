using System.Collections.Generic;
using System.Linq;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public class MethodEndpointRoutePolicy : IRoutePolicy
    {
        private readonly IEnumerable<IBuildEndpoints> _endpointFilters;

        public MethodEndpointRoutePolicy(params IBuildEndpoints[] endpointFilters)
        {
            _endpointFilters = endpointFilters;
        }

        public void BuildRoutes(IDictionary<string, object> environment)
        {
            var possibleEndpointMethods = AppDomainAssemblyTypeScanner.GetMethodsInAssemblies(environment.GetAssemblies());

            var possibleEndpoints = possibleEndpointMethods.Select(x => _endpointFilters.Select(y => y.Build(x)).FirstOrDefault(y => y != null)).Where(x => x != null).ToList();

            var baseNode = environment.GetRouteEngine().Base;

            foreach (var endpoint in possibleEndpoints)
            {
                var routeBuilder = environment.CreateRouteBuilder();

                if (routeBuilder == null)
                    continue;

                if (endpoint.HttpMethods.Any())
                    routeBuilder.RestrictMethods(endpoint.HttpMethods);

                foreach (var urlPart in endpoint.UrlParts)
                    urlPart.AddToBuilder(routeBuilder);

                routeBuilder.Build(baseNode, endpoint.Destination, endpoint.RoutedParameters, environment);
            }
        }
    }
}