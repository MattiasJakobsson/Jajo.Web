using System;
using System.Collections.Generic;

namespace SuperGlue
{
    internal static class ExceptionExtensions
    {
        internal class ExceptionConstants
        {
            public const string Exception = "superglue.Exception";
        }

        internal static Exception GetException(this IDictionary<string, object> environment)
        {
            return environment.Get<Exception>(ExceptionConstants.Exception);
        }
    }
}