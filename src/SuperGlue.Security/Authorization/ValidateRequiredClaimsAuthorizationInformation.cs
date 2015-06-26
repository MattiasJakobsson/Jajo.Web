using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Security.Authentication;

namespace SuperGlue.Security.Authorization
{
    public class ValidateRequiredClaimsAuthorizationInformation : IValidateAuthorizationInformation<RequiredClaimsAuthorizationInformation>
    {
        public Task<bool> IsValid(RequiredClaimsAuthorizationInformation authorizationInformation, IEnumerable<AuthenticationToken> tokens, IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var claims = tokens
                    .Where(x => string.IsNullOrEmpty(authorizationInformation.TokenSource) || x.Source == authorizationInformation.TokenSource)
                    .SelectMany(x => x.Claims)
                    .FirstOrDefault(x => string.IsNullOrEmpty(authorizationInformation.Key) || x.Type == authorizationInformation.Key);

                return claims != null && authorizationInformation.IsValid(claims.Value);
            });
        }
    }
}