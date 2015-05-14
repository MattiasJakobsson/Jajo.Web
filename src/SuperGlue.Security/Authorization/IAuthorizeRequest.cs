using System.Collections.Generic;
using SuperGlue.Security.Authentication;

namespace SuperGlue.Security.Authorization
{
    public interface IAuthorizeRequest
    {
        bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment);
    }
}