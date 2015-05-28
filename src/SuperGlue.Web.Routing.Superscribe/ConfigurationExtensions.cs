using SuperGlue.Configuration;
using SuperGlue.Web.Routing.Superscribe.Policies;

namespace SuperGlue.Web.Routing.Superscribe
{
    public static class ConfigurationExtensions
    {
        public static void UseRoutePolicy(this SuperGlueBootstrapper bootstrapper, IRoutePolicy policy)
        {
            policy.BuildRoutes(bootstrapper.Environment);
        }
    }
}