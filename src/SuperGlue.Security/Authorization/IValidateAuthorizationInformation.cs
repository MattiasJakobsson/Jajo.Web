using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Security.Authentication;

namespace SuperGlue.Security.Authorization
{
    public interface IValidateAuthorizationInformation<in TAuthorization> where TAuthorization : IAuthorizationInformation
    {
        Task<bool> IsValid(TAuthorization authorizationInformation, IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment);
    }
}