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
    }
}