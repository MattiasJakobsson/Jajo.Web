using System;

namespace SuperGlue.ExceptionManagement
{
    public class RetryException : Exception
    {
        public RetryException(int times, string description, Exception innerException) : base(string.Format("Failed {0} after {1} attempts", description, times), innerException)
        {
            
        } 
    }
}