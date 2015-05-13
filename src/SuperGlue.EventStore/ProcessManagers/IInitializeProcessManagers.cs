using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IInitializeProcessManagers
    {
        void Initialize(AppFunc chain);
        void Stop();
    }
}