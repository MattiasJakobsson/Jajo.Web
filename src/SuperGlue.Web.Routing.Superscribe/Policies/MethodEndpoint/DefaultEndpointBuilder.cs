using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public class DefaultEndpointBuilder : IBuildEndpoints
    {
        public EndpointInformation Build(MethodInfo method)
        {
            if (!IsValidEndpoint(method))
                return null;

            var httpMethods = new string[0];

            var methodNameMethod = GetMethodNameMethods();

            if (methodNameMethod.ContainsKey(method.Name))
                httpMethods = methodNameMethod[method.Name];

            var parts = new List<IUrlPart>();

            var handlerType = method.ReflectedType;
            var input = method.GetParameters().FirstOrDefault();

            if (handlerType != null)
            {
                var namespacePart = (handlerType.Namespace ?? "").Split('.').LastOrDefault();
                if (!string.IsNullOrEmpty(namespacePart) && !GetNamespacePartsToIgnore().Contains(namespacePart))
                    parts.Add(new StaticUrlPart(namespacePart.ToLower()));

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

                routedParameters[input.GetType()] = (x =>
                {
                    var allAvailableParameters = GetAvailableRouteParameters(x.GetType(), GetAllAvailableParameters());

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
            return new List<string> { "Index" };
        }

        protected virtual IEnumerable<string> GetParametersToAddBeforeEndpointName()
        {
            return new List<string> { "slug" };
        }

        protected virtual IEnumerable<string> GetParametersToAddAfterEndpointName()
        {
            return new List<string> { "id" };
        }

        protected virtual IEnumerable<string> GetAllAvailableParameters()
        {
            var parameters = GetParametersToAddBeforeEndpointName().ToList();
            parameters.AddRange(GetParametersToAddAfterEndpointName());
            
            return parameters;
        }
    }
}