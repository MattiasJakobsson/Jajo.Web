using System.Collections.Generic;

namespace SuperGlue.Web.Security.Authorization
{
    public interface IAuthorizeRequest
    {
        bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment);
    }
}