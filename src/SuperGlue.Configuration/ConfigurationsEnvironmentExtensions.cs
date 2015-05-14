using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Configuration
{
    public static class ConfigurationsEnvironmentExtensions
    {
        public static class ConfigurationConstants
        {
            public const string Assemblies = "superglue.Assemblies";
        }

        public static IEnumerable<Assembly> GetAssemblies(this IDictionary<string, object> environment)
        {
            return environment.Get<IEnumerable<Assembly>>(ConfigurationConstants.Assemblies);
        }
    }
}