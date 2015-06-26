using System;

namespace SuperGlue.Security.Authorization
{
    public class RequiredClaimsAuthorizationInformation : IAuthorizationInformation
    {
        public RequiredClaimsAuthorizationInformation(string tokenSource, string key = null, Func<string, bool> isValid = null)
        {
            Key = key;
            TokenSource = tokenSource;
            IsValid = isValid ?? (x => true);
        }

        public string Key { get; private set; }
        public string TokenSource { get; private set; }
        public Func<string, bool> IsValid { get; private set; }
    }
}