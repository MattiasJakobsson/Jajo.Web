using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public interface ISetupConfigurations
    {
        IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment);
        Task Shutdown(IDictionary<string, object> applicationData);
        Task Configure(SettingsConfiguration configuration);
    }
}