using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Configuration
{
    public class ConfigurationSetupResult
    {
        public ConfigurationSetupResult(string configurationName, Action<IDictionary<string, object>> action, string dependsOn = "superglue.ApplicationSetupStarted")
        {
            ConfigurationName = configurationName;
            Action = action;
            DependsOn = dependsOn;
        }

        public string ConfigurationName { get; private set; }
        public Action<IDictionary<string, object>> Action { get; private set; }
        public string DependsOn { get; private set; }
    }
}