using System.Collections.Generic;

namespace Jajo.Web.Configuration
{
    public interface IRunAtConfigurationTime
    {
        void Configure(IDictionary<string, object> applicationData);
    }
}