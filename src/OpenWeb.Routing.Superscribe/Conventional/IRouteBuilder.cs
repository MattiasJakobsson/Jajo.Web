using System.Reflection;
using Superscribe.Models;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public interface IRouteBuilder
    {
        void RestrictMethods(params string[] methods);
        void Append(string segment);
        void AppendParameter(RouteParameter parameter);

        GraphNode Build(GraphNode baseNode, MethodInfo routeTo);
    }
}