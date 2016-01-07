using System;

namespace SuperGlue.Configuration
{
    public class ApplicationStartupException : Exception
    {
        public ApplicationStartupException(int retries, Exception innerException) : base(string.Format("Failed to start applications after {0} retries", retries), innerException)
        {
            
        }
    }
}