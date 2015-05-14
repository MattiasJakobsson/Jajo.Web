using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IBuildAppFunction
    {
        IBuildAppFunction Use<TMiddleware>(params object[] args);
        AppFunc Build();
        IBuildAppFunction New();
    }
}