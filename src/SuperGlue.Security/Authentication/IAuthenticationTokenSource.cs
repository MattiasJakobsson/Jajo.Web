using System.Collections.Generic;

namespace SuperGlue.Security.Authentication
{
    public interface IAuthenticationTokenSource
    {
        string Name { get; }
        AuthenticationToken GetToken(IDictionary<string, object> environment);
        void SignOut(IDictionary<string, object> environment);
        void SigntOutBehalfOf(IDictionary<string, object> environment);
        AuthenticationInformation GetUser(IDictionary<string, object> environment);
        void SetAuthenticatedUser(AuthenticationToken token, IDictionary<string, object> environment);
        void SetOnBehalfOf(AuthenticationToken token, IDictionary<string, object> environment);
    }
}
