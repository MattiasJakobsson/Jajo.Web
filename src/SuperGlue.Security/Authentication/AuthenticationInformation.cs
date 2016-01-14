using System.Linq;

namespace SuperGlue.Security.Authentication
{
    public class AuthenticationInformation
    {
        public AuthenticationInformation(AuthenticationToken user, AuthenticationToken onBehalfOf)
        {
            OnBehalfOf = onBehalfOf;
            User = user;
        }

        public AuthenticationToken User { get; }
        public AuthenticationToken OnBehalfOf { get; }
        public bool IsAuthenticated => GetAuthorizedUser() != null;
        public bool IsBehalfOf => OnBehalfOf != null;

        public AuthenticationToken GetAuthorizedUser()
        {
            return OnBehalfOf ?? User;
        }

        public AuthenticationToken.Claim GetClaim(string key)
        {
            return GetAuthorizedUser().Claims.FirstOrDefault(x => x.Type == key);
        }
    }
}