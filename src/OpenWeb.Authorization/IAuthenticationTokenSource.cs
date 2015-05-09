using System.Collections.Generic;

namespace OpenWeb.Authorization
{
    public interface IAuthenticationTokenSource
    {
        AuthenticationToken GetToken(IDictionary<string, object> environment);
    }
}
