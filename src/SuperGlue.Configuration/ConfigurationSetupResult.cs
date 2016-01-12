using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class ConfigurationSetupResult
    {
        public ConfigurationSetupResult(string configurationName, Func<IDictionary<string, object>, Task> startupAction = null, string dependsOn = "superglue.ApplicationSetupStarted",
            Func<IDictionary<string, object>, Task> shutdownAction = null, Func<SettingsConfiguration, Task> configureAction = null, IEnumerable<string> applicationTags = null)
        {
            ConfigurationName = configurationName;
            StartupAction = startupAction ?? (x => Task.CompletedTask);
            ShutdownAction = shutdownAction ?? (x => Task.CompletedTask);
            ConfigureAction = configureAction ?? (x => Task.CompletedTask);
            DependsOn = dependsOn;
            ApplicationTags = applicationTags ?? new List<string>();
        }

        public string ConfigurationName { get; private set; }
        public Func<IDictionary<string, object>, Task> StartupAction { get; private set; }
        public Func<IDictionary<string, object>, Task> ShutdownAction { get; private set; }
        public Func<SettingsConfiguration, Task> ConfigureAction { get; private set; }
        public string DependsOn { get; private set; }
        public IEnumerable<string> ApplicationTags { get; private set; }
    }
}