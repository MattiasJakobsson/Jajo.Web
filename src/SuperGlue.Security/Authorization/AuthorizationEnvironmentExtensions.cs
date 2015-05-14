using System.Collections.Generic;

namespace SuperGlue.Security.Authorization
{
    public static class AuthorizationEnvironmentExtensions
    {
        private const string AuthorizationFailed = "superglue.AuthorizationFailed";

        public static bool HasAuthorizationFailed(this IDictionary<string, object> environment)
        {
            return environment.Get<bool>(AuthorizationFailed);
        }

        public static void FailAuthorization(this IDictionary<string, object> environment)
        {
            environment[AuthorizationFailed] = true;
        }
    }
}