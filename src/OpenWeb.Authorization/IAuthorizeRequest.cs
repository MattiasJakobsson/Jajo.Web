using System.Collections.Generic;

namespace OpenWeb.Authorization
{
    public interface IAuthorizeRequest
    {
        bool IsAuthorized(IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment);
    }
}