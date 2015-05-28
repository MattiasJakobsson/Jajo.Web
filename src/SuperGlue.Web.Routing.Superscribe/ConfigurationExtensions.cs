using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.Web.Routing.Superscribe.Policies;

namespace SuperGlue.Web.Routing.Superscribe
{
    public static class ConfigurationExtensions
    {
        public static void UseRoutePolicy(this SuperGlueBootstrapper bootstrapper, IRoutePolicy policy, IDictionary<string, object> environment)
        {
            policy.BuildRoutes(environment);
        }
    }
}