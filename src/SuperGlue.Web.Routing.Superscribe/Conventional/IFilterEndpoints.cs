using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
{
    public interface IFilterEndpoints
    {
        bool IsValidEndpoint(MethodInfo method);
    }
}