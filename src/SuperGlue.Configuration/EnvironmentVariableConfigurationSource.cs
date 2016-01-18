using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Configuration
{
    public class EnvironmentVariableConfigurationSource : IConfigurationSource
    {
        public const string ConnectionStringPrefix = "CONNECTIONSTRING_";

        public IEnumerable<string> GetAvailableSettingKeys()
        {
            return Environment.GetEnvironmentVariables().Keys.OfType<string>().Where(x => !x.StartsWith(ConnectionStringPrefix));
        }

        public bool HasSetting(string key)
        {
            return !string.IsNullOrEmpty(GetSetting(key));
        }

        public string GetSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        public bool HasConnectionString(string name)
        {
            return !string.IsNullOrEmpty(GetConnectionString(name));
        }

        public string GetConnectionString(string name)
        {
            return Environment.GetEnvironmentVariable($"{ConnectionStringPrefix}{name}");
        }
    }
}