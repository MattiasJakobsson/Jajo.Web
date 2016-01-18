using System.Collections.Generic;
using System.Configuration;

namespace SuperGlue.Configuration
{
    public class ConfigFileConfigurationSource : IConfigurationSource
    {
        public IEnumerable<string> GetAvailableSettingKeys()
        {
            return ConfigurationManager.AppSettings.AllKeys;
        }

        public bool HasSetting(string key)
        {
            return !string.IsNullOrEmpty(GetSetting(key));
        }

        public string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public bool HasConnectionString(string name)
        {
            return !string.IsNullOrEmpty(GetConnectionString(name));
        }

        public string GetConnectionString(string key)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[key];

            return connectionString?.ConnectionString;
        }
    }
}