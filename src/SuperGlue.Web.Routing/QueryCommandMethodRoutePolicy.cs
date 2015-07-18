using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Routing
{
    public class QueryCommandMethodRoutePolicy : IRoutePolicy
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public QueryCommandMethodRoutePolicy(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<EndpointInformation> Build()
        {
            var possibleEndpoints = _assemblies.SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods()).Where(IsValidEndpoint).ToList();

            return possibleEndpoints.Select(Build);
        }

        protected virtual EndpointInformation Build(MethodInfo method)
        {
            var httpMethods = new string[0];

            var methodNameMethod = GetMethodNameMethods();

            if (methodNameMethod.ContainsKey(method.Name))
                httpMethods = methodNameMethod[method.Name];

            var parts = GetInitialUrlParts().ToList();

            var handlerType = method.ReflectedType;
            var input = method.GetParameters().FirstOrDefault();

            if (handlerType != null)
            {
                var namespaceParts = GetNamespaceParts(handlerType.Namespace ?? "");

                parts.AddRange((from namespacePart in namespaceParts 
                                where !string.IsNullOrEmpty(namespacePart) && !GetNamespacePartsToIgnore().Contains(namespacePart) 
                                select new StaticUrlPart(namespacePart.ToLower())));

                if (input != null)
                {
                    var availableParameters = GetAvailableRouteParameters(input.ParameterType, GetParametersToAddBeforeEndpointName());

                    parts.AddRange(availableParameters.Select(parameter => new ParameterUrlPart(parameter)));
                }

                if (!GetEndpointNamesToIgnore().Contains(handlerType.Name))
                    parts.Add(new StaticUrlPart(handlerType.Name.ToLower()));
            }

            var routedParameters = new Dictionary<Type, Func<object, IDictionary<string, object>>>();

            if (input != null)
            {
                var availableParameters = GetAvailableRouteParameters(input.ParameterType, GetParametersToAddAfterEndpointName());

                parts.AddRange(availableParameters.Select(parameter => new ParameterUrlPart(parameter)));

                var allAvailableParameters = GetAvailableRouteParameters(input.ParameterType, GetAllAvailableParameters()).ToList();
                allAvailableParameters.AddRange(GetAvailableQueryStringParameters(input.ParameterType));

                routedParameters[input.ParameterType] = (x =>
                {
                    return allAvailableParameters.ToDictionary(y => y.Name, y => y.GetValue(x));
                });
            }

            return new EndpointInformation(method, parts, routedParameters, httpMethods);
        }

        protected virtual bool IsValidEndpoint(MethodInfo method)
        {
            return method.Name == "Query" || method.Name == "Command";
        }

        protected virtual IEnumerable<RouteParameter> GetAvailableRouteParameters(Type input, IEnumerable<string> availableParameterNames)
        {
            return input.GetProperties().Where(x => availableParameterNames.Any(y => y.Equals(x.Name, StringComparison.OrdinalIgnoreCase))).Select(RouteParameter.FromProperty).ToList();
        }

        protected virtual IEnumerable<RouteParameter> GetAvailableQueryStringParameters(Type input)
        {
            var availableUrlParameterNames = GetAllAvailableParameters();
            return input.GetProperties().Where(x => !availableUrlParameterNames.Any(y => y.Equals(x.Name, StringComparison.OrdinalIgnoreCase))).Select(RouteParameter.FromProperty).ToList();
        }

        protected virtual IEnumerable<string> GetNamespaceParts(string ns)
        {
            yield return ns.Split('.').LastOrDefault();
        }

        protected virtual IEnumerable<IUrlPart> GetInitialUrlParts()
        {
            return new List<IUrlPart>();
        }

        protected virtual IReadOnlyDictionary<string, string[]> GetMethodNameMethods()
        {
            return new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>
            {
                {"Query", new[] {"GET"}},
                {"Command", new[] {"POST"}}
            });
        }

        protected virtual IEnumerable<string> GetNamespacePartsToIgnore()
        {
            return new List<string> { "Home" };
        }

        protected virtual IEnumerable<string> GetEndpointNamesToIgnore()
        {
            return new List<string> { "Index", "Details" };
        }

        protected virtual IEnumerable<string> GetParametersToAddBeforeEndpointName()
        {
            return new List<string> { "slug" };
        }

        protected virtual IEnumerable<string> GetParametersToAddAfterEndpointName()
        {
            return new List<string> { "id", "step" };
        }

        protected virtual IEnumerable<string> GetAllAvailableParameters()
        {
            var parameters = GetParametersToAddBeforeEndpointName().ToList();
            parameters.AddRange(GetParametersToAddAfterEndpointName());
            
            return parameters;
        }
    }
}