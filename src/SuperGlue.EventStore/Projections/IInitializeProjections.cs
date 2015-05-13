using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IInitializeProjections
    {
        void Initialize(AppFunc chain);
        void Stop();
    }
}