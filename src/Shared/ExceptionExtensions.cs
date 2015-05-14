using System;
using System.Collections.Generic;

namespace SuperGlue
{
    internal static class ExceptionExtensions
    {
        public static Exception GetException(this IDictionary<string, object> environment)
        {
            return environment.Get<Exception>("superglue.Exception");
        }
    }
}