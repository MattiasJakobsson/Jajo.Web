using System.Reflection;

namespace Jajo.Web.Routing.Superscribe.Conventional
{
    public interface IFilterEndpoints
    {
        bool IsValidEndpoint(MethodInfo method);
    }
}