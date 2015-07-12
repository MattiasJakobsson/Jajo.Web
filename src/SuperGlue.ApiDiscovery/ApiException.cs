using System;

namespace SuperGlue.ApiDiscovery
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
            
        }
    }
}