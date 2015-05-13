using System.Collections.Generic;

namespace SuperGlue.Security.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticationResult CheckAuthentication<TAuthenticationRequest>(TAuthenticationRequest authenticationRequest, IDictionary<string, object> environment) where TAuthenticationRequest : IAuthenticationRequest;
        void SignOut(IDictionary<string, object> environment);
        void SigntOutBehalfOf(IDictionary<string, object> environment);
        AuthenticationInformation GetUser(IDictionary<string, object> environment);
    }
}