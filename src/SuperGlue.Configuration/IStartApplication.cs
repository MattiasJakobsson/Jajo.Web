using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IStartApplication
    {
        void Start(IDictionary<string, AppFunc> chains, IDictionary<string, object> environment);
        void ShutDown();
    }
}