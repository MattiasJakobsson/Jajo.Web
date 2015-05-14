using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public static class SubApplicationsEnvironmentExtensions
    {
        public static class SubApplicationConstants
        {
            public const string SubApplications = "superglue.SubApplications";
        }

        public static IEnumerable<InitializedSubApplication> GetSubApplications(this IDictionary<string, object> environment)
        {
            return environment.Get<IEnumerable<InitializedSubApplication>>(SubApplicationConstants.SubApplications);
        }
    }
}