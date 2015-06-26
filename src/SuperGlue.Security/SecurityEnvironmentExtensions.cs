using System.Collections.Generic;

namespace SuperGlue.Security
{
    public static class SecurityEnvironmentExtensions
    {
        public static class SecurityConstants
        {
            public const string HasherSettings = "superglue.Security.HasherSettings";
        }

        public static HasherSettings GetHasherSettings(this IDictionary<string, object> environment)
        {
            return environment.Get(SecurityConstants.HasherSettings, new HasherSettings());
        }
    }
}