using System;

namespace SuperGlue.Configuration
{
    public interface IWrapMiddleware<TMiddleware>
    {
        IDisposable Begin();
    }
}