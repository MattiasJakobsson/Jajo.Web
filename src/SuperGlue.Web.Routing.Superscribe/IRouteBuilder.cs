using System;
using System.Collections.Generic;
using Superscribe.Models;

namespace SuperGlue.Web.Routing.Superscribe
{
    public interface IRouteBuilder
    {
        void RestrictMethods(params string[] methods);
        void Append(string segment);
        void AppendParameter(RouteParameter parameter);
        void AppendPattern(string pattern);

        GraphNode Build(GraphNode baseNode, object routeTo, IDictionary<Type, Func<object, IDictionary<string, object>>> routedInputs, IDictionary<string, object> environment);
    }
}