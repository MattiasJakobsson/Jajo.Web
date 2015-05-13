using System.Collections.Generic;

namespace SuperGlue.Web.Configuration
{
    public interface IRunAtConfigurationTime
    {
        void Configure(IDictionary<string, object> applicationData);
        void Shutdown(IDictionary<string, object> applicationData);
    }
}