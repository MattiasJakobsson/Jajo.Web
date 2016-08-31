using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public interface IStartApplication
    {
        string Name { get; }
        string Chain { get; }
        Task Start(AppFunc chain, IDictionary<string, object> settings, string environment, string[] arguments);
        Task ShutDown(IDictionary<string, object> settings);
        AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment);
    }
}