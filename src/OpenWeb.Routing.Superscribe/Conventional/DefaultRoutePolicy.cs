using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public class DefaultRoutePolicy : IRoutePolicy
    {
        private readonly IDictionary<string, string[]> _methodNameMethod = new Dictionary<string, string[]>
        {
            {"Query", new []{"GET"}},
            {"Command", new []{"POST"}}
        };

        private readonly IEnumerable<string> _availableParameterNames;

        private readonly IEnumerable<string> _namespacePartsToIgnore = new List<string> { "Home" };
        private readonly IEnumerable<string> _handlerNamesToIgnore = new List<string> { "Index" };

        public DefaultRoutePolicy(IEnumerable<string> availableParameterNames = null)
        {
            _availableParameterNames = availableParameterNames ?? new List<string>
            {
                "slug",
                "id"
            };
        }

        public bool Matches(MethodInfo endpoint)
        {
            return true;
        }

        public void Build(MethodInfo endpoint, IRouteBuilder builder)
        {
            if (_methodNameMethod.ContainsKey(endpoint.Name))
                builder.RestrictMethods(_methodNameMethod[endpoint.Name]);

            var handlerType = endpoint.ReflectedType;

            if (handlerType != null)
            {
                var namespacePart = (handlerType.Namespace ?? "").Split('.').LastOrDefault();
                if (!string.IsNullOrEmpty(namespacePart) && !_namespacePartsToIgnore.Contains(namespacePart))
                    builder.Append(namespacePart.ToLower());

                if (!_handlerNamesToIgnore.Contains(handlerType.Name))
                    builder.Append(handlerType.Name.ToLower());
            }

            var input = endpoint.GetParameters().FirstOrDefault();
            if (input == null) return;

            var inputParameters = GetAvailableRouteParameters(input.ParameterType);
            foreach (var parameter in inputParameters)
                builder.AppendParameter(parameter);
        }

        public IEnumerable<RouteParameter> GetAvailableRouteParameters(Type input)
        {
            return input.GetProperties().Where(x => _availableParameterNames.Any(y => y.Equals(x.Name, StringComparison.OrdinalIgnoreCase))).Select(RouteParameter.FromProperty).ToList();
        }
    }
}