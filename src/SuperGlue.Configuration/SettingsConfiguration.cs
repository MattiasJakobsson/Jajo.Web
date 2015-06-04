using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SettingsConfiguration
    {
        private readonly Func<Type, object> _getSettings;

        public SettingsConfiguration(Func<Type, object> getSettings, IDictionary<string, object> environment)
        {
            _getSettings = getSettings;
            Environment = environment;
        }

        public IDictionary<string, object> Environment { get; private set; }

        public TSettings WithSettings<TSettings>()
        {
            return (TSettings)_getSettings(typeof (TSettings));
        }
    }
}