using System.Reflection;

namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public interface IBuildEndpoints
    {
        EndpointInformation Build(MethodInfo method);
    }
}