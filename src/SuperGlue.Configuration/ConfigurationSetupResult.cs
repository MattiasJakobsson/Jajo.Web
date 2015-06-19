using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class ConfigurationSetupResult
    {
        public ConfigurationSetupResult(string configurationName, Action<IDictionary<string, object>> action, string dependsOn = "superglue.ApplicationSetupStarted", string environment = null)
        {
            ConfigurationName = configurationName;
            Action = action;
            Environment = environment;
            DependsOn = dependsOn;
        }

        public string ConfigurationName { get; private set; }
        public Action<IDictionary<string, object>> Action { get; private set; }
        public string DependsOn { get; private set; }
        public string Environment { get; private set; }
    }
}