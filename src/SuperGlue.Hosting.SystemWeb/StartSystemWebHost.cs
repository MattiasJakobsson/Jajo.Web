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
            return Task.Factory.StartNew(() =>
            {
                var appBuilder = settings.Get<IAppBuilder>(SystemWebEnvironmentConstants.AppBuilder);

                appBuilder.Use<RunAppFunc>(new RunAppFuncOptions(chain));
            });
        }

        public Task ShutDown()
        {
            return Task.Factory.StartNew(() => { });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return null;
        }
    }
}