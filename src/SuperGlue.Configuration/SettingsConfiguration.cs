using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SettingsConfiguration
    {
        private readonly Func<Type, object> _getSettings;

        public SettingsConfiguration(Func<Type, object> getSettings, IDictionary<string, object> settings, string environment)
        {
            _getSettings = getSettings;
            Settings = settings;
            Environment = environment;
        }

        public IDictionary<string, object> Settings { get; private set; }
        public string Environment { get; private set; }

        public TSettings WithSettings<TSettings>()
        {
            return (TSettings)_getSettings(typeof (TSettings));
        }

        public void InitializeSettings<TSettings>()
        {
            _getSettings(typeof (TSettings));
        }
    }
}