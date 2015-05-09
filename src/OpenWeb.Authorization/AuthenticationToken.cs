using System.Collections.Generic;

namespace OpenWeb.Authorization
{
    public class AuthenticationToken
    {
        public AuthenticationToken(IEnumerable<Claim> claims)
        {
            Claims = claims;
        }

        public IEnumerable<Claim> Claims { get; private set; }

        public class Claim
        {
            public Claim(string type, string value)
            {
                Type = type;
                Value = value;
            }

            public string Type { get; private set; }
            public string Value { get; private set; }
        } 
    }
}