using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public interface IApplicationConfiguration
    {
        IEnumerable<string> GetAvailableSettingKeys();

        string GetSetting(string key);
        string GetConnectionString(string key);
    }
}