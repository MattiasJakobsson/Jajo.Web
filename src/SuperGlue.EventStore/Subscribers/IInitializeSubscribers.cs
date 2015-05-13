using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Subscribers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IInitializeSubscribers
    {
        void Initialize(AppFunc chain, string name);
        void Stop();
    }
}