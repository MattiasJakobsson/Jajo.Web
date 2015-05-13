using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Superscribe.Engine;
using Superscribe.Models;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
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

            var endpointRoutes = new Dictionary<MethodInfo, GraphNode>();
            var routePolicyByEndpoint = new Dictionary<MethodInfo, IRoutePolicy>();

            foreach (var endpoint in possibleEndpoints)
            {
                var matchingPolicy = _policies.FirstOrDefault(x => x.Matches(endpoint));

                if (matchingPolicy == null)
                    continue;

                var routeBuilder = new RouteBuilder();

                matchingPolicy.Build(endpoint, routeBuilder);

                endpointRoutes[endpoint] = routeBuilder.Build(define.Base, endpoint);
                routePolicyByEndpoint[endpoint] = matchingPolicy;
            }

            environmentSettings["superglue.Superscribe.Engine"] = define;

            var inputToEndpoint = endpointRoutes
                .Where(x => x.Key.GetParameters().Length == 1)
                .ToDictionary(x => x.Key.GetParameters()[0].ParameterType, x => x.Key);

            environmentSettings["superglue.RoutedEndpoints.Inputs"] = inputToEndpoint;

            environmentSettings["superglue.RoutedEnpoints.ParametersFromInput"] = (Func<object, IDictionary<string, object>>) (x =>
                {
                    if(!inputToEndpoint.ContainsKey(x.GetType()))
                        return new Dictionary<string, object>();

                    var endpoint = inputToEndpoint[x.GetType()];

                    if(!routePolicyByEndpoint.ContainsKey(endpoint))
                        return new Dictionary<string, object>();

                    var routePolicy = routePolicyByEndpoint[endpoint];

                    return routePolicy
                        .GetAvailableRouteParameters(x.GetType())
                        .ToDictionary(y => y.Name, y => y.GetValue(x));
                });

            environmentSettings["superglue.ReverseRoute"] = (Func<object, IDictionary<string, object>, string>) ((endpoint, parameters) =>
            {
                var method = endpoint as MethodInfo;

                if (method == null)
                    return "";

                if (!endpointRoutes.ContainsKey(method))
                    return "";

                var node = endpointRoutes[method].Base();

                var patternBuilder = new StringBuilder();

                while (node != null)
                {
                    patternBuilder.Append("/");

                    var paramNode = node as ParamNode;

                    if (paramNode != null)
                    {
                        if (parameters.ContainsKey(paramNode.Name))
                            patternBuilder.Append(parameters[paramNode.Name]);
                    }
                    else
                    {
                        patternBuilder.Append(node.Template);
                    }

                    node = node.Edges.FirstOrDefault();
                }

                return patternBuilder.ToString();
            });

            return define;
        }
    }
}