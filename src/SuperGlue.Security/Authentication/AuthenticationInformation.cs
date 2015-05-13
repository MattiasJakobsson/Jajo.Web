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

        public AuthenticationToken User { get; private set; }
        public AuthenticationToken OnBehalfOf { get; private set; }
        public bool IsAuthenticated { get { return GetAuthorizedUser() != null; } }
        public bool IsBehalfOf { get { return OnBehalfOf != null; } }

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