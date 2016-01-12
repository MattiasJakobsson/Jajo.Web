using System;

namespace SuperGlue.Configuration
{
    [Serializable]
    public class ApplicationStartupException : Exception
    {
        public ApplicationStartupException(int retries, Exception innerException) : base(
            $"Failed to start applications after {retries} retries", innerException)
        {
            
        }
    }
}