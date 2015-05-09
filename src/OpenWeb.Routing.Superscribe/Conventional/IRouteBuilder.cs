using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Superscribe.Models;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public interface IRouteBuilder
    {
        void RestrictMethods(params string[] methods);
        void Append(string segment);
        void AppendParameter(PropertyInfo parameter);

        GraphNode Build(GraphNode baseNode, MethodInfo routeTo);
    }

    public interface IRoutePolicy
    {
        bool Matches(MethodInfo endpoint);
        void Build(MethodInfo endpoint, IRouteBuilder builder);
    }

    public class DefaultRoutePolicy : IRoutePolicy
    {
        private readonly IDictionary<string, string[]> _methodNameMethod = new Dictionary<string, string[]>
                                                                              {
                                                                                  {"Query", new []{"GET"}},
                                                                                  {"Command", new []{"POST"}}
                                                                              };

        private readonly IEnumerable<string> _namespacePartsToIgnore = new List<string> { "Home" };
        private readonly IEnumerable<string> _handlerNamesToIgnore = new List<string> { "Index" };

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

            var inputParameters = input.ParameterType.GetProperties();
            foreach (var parameter in inputParameters)
                builder.AppendParameter(parameter);
        }
    }

}