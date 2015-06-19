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
            public const string ResolvePathFunc = "superglue.Configuration.ResolvePath";
        }

        public static IEnumerable<Assembly> GetAssemblies(this IDictionary<string, object> environment)
        {
            return environment.Get<ICollection<Assembly>>(ConfigurationConstants.Assemblies);
        }

        public static void AddAssembly(this IDictionary<string, object> environment, Assembly assembly)
        {
            var assemblies = environment.Get<ICollection<Assembly>>(ConfigurationConstants.Assemblies);

            if(!assemblies.Contains(assembly))
                assemblies.Add(assembly);
        }

        public static void AlterSettings<TSettings>(this IDictionary<string, object> environment, Action<TSettings> alterer)
        {
            var alter = environment.Get<Action<Type, Action<object>>>(ConfigurationConstants.AlterConfigSettings);

            alter(typeof(TSettings), x => alterer((TSettings)x));
        }

        public static string ResolvePath(this IDictionary<string, object> environment, string relativePath)
        {
            return environment.Get<Func<string, string>>(ConfigurationConstants.ResolvePathFunc)(relativePath);
        }
    }
}