using System.Collections.Generic;

namespace SuperGlue.Web.Authorization
{
    public interface IAuthenticationTokenSource
    {
        AuthenticationToken GetToken(IDictionary<string, object> environment);
    }
}
