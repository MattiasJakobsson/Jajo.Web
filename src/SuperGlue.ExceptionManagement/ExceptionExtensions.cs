using System;
using System.Collections.Generic;

namespace SuperGlue.ExceptionManagement
{
    public static class ExceptionExtensions
    {
        internal class ExceptionConstants
        {
            public const string Exception = "superglue.Exception";
        }

        public static Exception GetException(this IDictionary<string, object> environment)
        {
            return environment.Get<Exception>(ExceptionConstants.Exception);
        }
    }
}