using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Routing
{
    public interface IRouteBuilder
    {
        void RestrictMethods(params string[] methods);
        void Append(string segment);
        void AppendParameter(RouteParameter parameter);
        void AppendPattern(string pattern);
        void AddConstraint(IRouteConstraint constraint);

        void Build(object routeTo, IDictionary<Type, Func<object, IDictionary<string, object>>> routedInputs, IDictionary<string, object> environment);
    }
}