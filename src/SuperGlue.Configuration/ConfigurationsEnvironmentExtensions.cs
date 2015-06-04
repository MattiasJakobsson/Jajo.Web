using System;
using System.Collections.Generic;
using System.Reflection;

namespace SuperGlue.Configuration
{
    public static class ConfigurationsEnvironmentExtensions
    {
        public static class ConfigurationConstants
        {
            public const string Assemblies = "superglue.Assemblies";
            public const string AlterConfigSettings = "superglue.Configuration.AlterSettings";
        }

        public static IEnumerable<Assembly> GetAssemblies(this IDictionary<string, object> environment)
        {
            return environment.Get<IEnumerable<Assembly>>(ConfigurationConstants.Assemblies);
        }

        public static void AlterSettings<TSettings>(this IDictionary<string, object> environment, Action<TSettings> alterer)
        {
            var alter = environment.Get<Action<Type, Action<object>>>(ConfigurationConstants.AlterConfigSettings);

            alter(typeof(TSettings), x => alterer((TSettings)x));
        }
    }
}