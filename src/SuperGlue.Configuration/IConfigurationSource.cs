using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public interface IConfigurationSource
    {
        IEnumerable<string> GetAvailableSettingKeys();

        bool HasSetting(string key);
        string GetSetting(string key);
        bool HasConnectionString(string name);
        string GetConnectionString(string name);
    }
}