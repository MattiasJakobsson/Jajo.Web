using System;
using System.Collections.Generic;

namespace Jajo.Web
{
    internal static class ExceptionExtensions
    {
        public static Exception GetException(this IDictionary<string, object> environment)
        {
            return environment.Get<Exception>("jajo.Exception");
        }
    }
}