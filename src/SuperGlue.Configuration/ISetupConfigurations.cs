using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public interface ISetupConfigurations
    {
        IEnumerable<ConfigurationSetupResult> Setup();
        void Shutdown(IDictionary<string, object> applicationData);
        void Configure(SettingsConfiguration configuration);
    }
}