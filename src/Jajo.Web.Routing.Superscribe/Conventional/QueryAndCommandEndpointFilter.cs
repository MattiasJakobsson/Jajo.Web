using System.Reflection;

namespace Jajo.Web.Routing.Superscribe.Conventional
{
    public class QueryAndCommandEndpointFilter : IFilterEndpoints
    {
        public bool IsValidEndpoint(MethodInfo method)
        {
            return method.Name == "Query" || method.Name == "Command";
        }
    }
}