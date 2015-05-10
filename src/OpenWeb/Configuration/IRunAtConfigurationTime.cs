using System.Collections.Generic;

namespace OpenWeb.Configuration
{
    public interface IRunAtConfigurationTime
    {
        void Configure(IDictionary<string, object> applicationData);
    }
}