using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class ChainSettings
    {
        private readonly IDictionary<string, object> _settings = new Dictionary<string, object>();

        public ChainSettings UseSetting(string key, object value)
        {
            _settings[key] = value;

            return this;
        }

        public object GetSetting(string key, object defaultValue = null)
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public T GetSetting<T>(string key, T defaultValue = null) where T : class
        {
            return _settings.ContainsKey(key) ? _settings[key] as T : defaultValue;
        }
    }
}