using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public interface IWrapMiddleware<TMiddleware>
    {
        Task<IDisposable> Begin(IDictionary<string, object> environment, Type middlewareType);
    }
}