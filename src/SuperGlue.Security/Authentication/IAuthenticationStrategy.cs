using System.Collections.Generic;

namespace SuperGlue.Security.Authentication
{
    public interface IAuthenticationStrategy<in TAuthenticationRequest> : IAuthenticationStrategy where TAuthenticationRequest : IAuthenticationRequest
    {
        AuthenticationResult CheckAuthentication(TAuthenticationRequest authenticationRequest, IDictionary<string, object> environment);
    }

    public interface IAuthenticationStrategy
    {
        string Name { get; }
        void SignOut(IDictionary<string, object> environment);
        void SigntOutBehalfOf(IDictionary<string, object> environment);
        AuthenticationInformation GetUser(IDictionary<string, object> environment);
        void SetAuthenticatedUser(AuthenticationToken token, IDictionary<string, object> environment);
        void SetOnBehalfOf(AuthenticationToken token, IDictionary<string, object> environment);
    }
}