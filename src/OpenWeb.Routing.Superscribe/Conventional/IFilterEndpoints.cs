using System.Reflection;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public interface IFilterEndpoints
    {
        bool IsValidEndpoint(MethodInfo method);
    }
}