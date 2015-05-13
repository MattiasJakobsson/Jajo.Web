using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Configuration
{
    public static class Configurations
    {
        public static IDictionary<string, object> Configure(IEnumerable<Assembly> assemblies)
        {
            var environment = new Dictionary<string, object>
            {
                {"superglue.Assemblies", assemblies}
            };

            var configurations = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof (ISetupConfigurations).IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .OfType<ISetupConfigurations>()
                .SelectMany(x => x.Setup())
                .ToList();

            ExecuteConfigurationsDependingOn(new ReadOnlyCollection<ConfigurationSetupResult>(configurations), "superglue.ApplicationSetupStarted", environment);

            return environment;
        }

        private static void ExecuteConfigurationsDependingOn(IReadOnlyCollection<ConfigurationSetupResult> configurations, string dependsOn, IDictionary<string, object> environment)
        {
            var configurationsToExecute = configurations.Where(x => x.DependsOn == dependsOn).ToList();

            var results = new List<string>();

            foreach (var configuration in configurationsToExecute)
            {
                configuration.Action(environment);
                results.Add(configuration.ConfigurationName);
            }

            foreach (var item in results.Distinct())
            {
                ExecuteConfigurationsDependingOn(configurations, item, environment);
            }
        }
    }
}