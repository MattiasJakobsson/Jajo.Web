using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class SubAppsConfiguration
    {
        public SubAppsConfiguration()
        {
            SubApplications = new List<SubApplication>();
        }

        public IEnumerable<SubApplication> SubApplications { get; set; }
    }
}