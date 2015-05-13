using System.Collections.Generic;

namespace SuperGlue.Web.Security.Authorization
{
    public interface IAuthenticationTokenSource
    {
        AuthenticationToken GetToken(IDictionary<string, object> environment);
    }
}
