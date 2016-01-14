using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Security.Authentication
{
    public class AuthenticationToken
    {
        public AuthenticationToken(string source, IEnumerable<Claim> claims)
        {
            Claims = claims;
            Source = source;
        }

        public string Source { get; private set; }
        public IEnumerable<Claim> Claims { get; }

        public class Claim
        {
            public Claim(string type, string value)
            {
                Type = type;
                Value = value;
            }

            public string Type { get; }
            public string Value { get; }
        }

        public string GetClaimsString()
        {
            return string.Join(";", Claims.Select(x => string.Concat(x.Type, "=", x.Value)));
        }
    }
}