using System.Reflection;
using Superscribe.Models;

namespace OpenWeb.Routing.Superscribe
{
    public static class GraphNodeExtensions
    {
        public static GraphNode RouteTo(this GraphNode node, MethodInfo method)
        {
            node.ActionFunctions.Add("OpenWeb", (routeData, x) =>
            {
                routeData.Environment["route.RoutedTo"] = method;
            });

            return node;
        }
    }
}