using System.Collections.Generic;

namespace Jajo.Web.Authorization
{
    public interface IAuthenticationTokenSource
    {
        AuthenticationToken GetToken(IDictionary<string, object> environment);
    }
}
