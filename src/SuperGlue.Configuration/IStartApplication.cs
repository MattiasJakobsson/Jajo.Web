using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IStartApplication
    {
        string Chain { get; }
        void Start(AppFunc chain, IDictionary<string, object> environment);
        void ShutDown();
        AppFunc GetDefaultChain(IBuildAppFunction buildApp);
    }
}