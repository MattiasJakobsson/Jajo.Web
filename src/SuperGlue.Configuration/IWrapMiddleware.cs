using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public interface IWrapMiddleware<TMiddleware>
    {
        IDisposable Begin(IDictionary<string, object> environment);
    }
}