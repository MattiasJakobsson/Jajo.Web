namespace SuperGlue.Security.Authentication
{
    public class AuthenticationResult
    {
        public AuthenticationResult(AuthenticationToken token, string description, int code)
        {
            Token = token;
            Description = description;
            Code = code;
        }

        public AuthenticationToken Token { get; }
        public string Description { get; private set; }
        public int Code { get; private set; }
        public bool Success => Token != null;
    }
}