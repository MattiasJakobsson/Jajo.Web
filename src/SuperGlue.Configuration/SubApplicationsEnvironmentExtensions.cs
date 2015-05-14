using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public static class SubApplicationsEnvironmentExtensions
    {
        public static IEnumerable<InitializedSubApplication> GetSubApplications(this IDictionary<string, object> environment)
        {
            return environment.Get<IEnumerable<InitializedSubApplication>>("superglue.SubApplications");
        }
    }
}