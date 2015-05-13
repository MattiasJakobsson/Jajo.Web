using System.Collections.Generic;

namespace SuperGlue.Web.Configuration
{
    public interface ISetupConfigurations
    {
        IEnumerable<ConfigurationSetupResult> Setup();
        void Shutdown(IDictionary<string, object> applicationData);
    }
}