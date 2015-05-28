using System.Reflection;
using Superscribe.Models;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
{
    public interface IRouteBuilder
    {
        void RestrictMethods(params string[] methods);
        void Append(string segment);
        void AppendParameter(RouteParameter parameter);
        void AppendPattern(string pattern);

        GraphNode Build(GraphNode baseNode, object routeTo);
    }
}