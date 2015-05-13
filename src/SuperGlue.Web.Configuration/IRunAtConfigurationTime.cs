using System.Collections.Generic;

namespace SuperGlue.Web.Configuration
{
    public interface IRunAtConfigurationTime
    {
        void Configure(IDictionary<string, object> applicationData);
    }
}