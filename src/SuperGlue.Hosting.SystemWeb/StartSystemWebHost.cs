using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.SystemWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartSystemWebHost : IStartApplication
    {
        public string Chain { get { return "chains.Web"; } }

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            var appBuilder = settings.Get<IAppBuilder>(SystemWebEnvironmentConstants.AppBuilder);

            appBuilder.Use<RunAppFunc>(new RunAppFuncOptions(chain));

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return null;
        }
    }
}