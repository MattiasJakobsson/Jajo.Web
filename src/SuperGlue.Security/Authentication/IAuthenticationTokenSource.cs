using System.Collections.Generic;

namespace SuperGlue.Security.Authentication
{
    public interface IAuthenticationTokenSource
    {
        AuthenticationToken GetToken(IDictionary<string, object> environment);
    }
}
