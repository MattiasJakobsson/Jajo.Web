using System.Collections.Generic;

namespace SuperGlue.Security.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticationResult CheckAuthentication<TAuthenticationRequest>(TAuthenticationRequest authenticationRequest, IDictionary<string, object> environment) where TAuthenticationRequest : IAuthenticationRequest;
        void SignOut(IDictionary<string, object> environment, string source = null);
        void SigntOutBehalfOf(IDictionary<string, object> environment, string source = null);
        AuthenticationInformation GetUser(IDictionary<string, object> environment, string source = null);
        IEnumerable<AuthenticationInformation> GetUsers(IDictionary<string, object> environment);
        void SetAuthenticatedUser(AuthenticationToken token, IDictionary<string, object> environment);
        void SetOnBehalfOf(AuthenticationToken token, IDictionary<string, object> environment);
    }
}