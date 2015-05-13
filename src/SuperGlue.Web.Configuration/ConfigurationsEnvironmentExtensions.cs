using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Web.Configuration
{
    public static class ConfigurationsEnvironmentExtensions
    {
        public static IEnumerable<Assembly> GetAssemblies(this IDictionary<string, object> environment)
        {
            return environment.Get<IEnumerable<Assembly>>("superglue.Assemblies");
        }
    }
}