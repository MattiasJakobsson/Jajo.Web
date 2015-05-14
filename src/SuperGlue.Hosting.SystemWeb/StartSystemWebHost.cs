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

        public void Start(AppFunc chain, IDictionary<string, object> environment)
        {
            var appBuilder = environment.Get<IAppBuilder>(SystemWebEnvironmentConstants.AppBuilder);

            appBuilder.Use<RunAppFunc>(new RunAppFuncOptions(chain, environment));
        }

        public void ShutDown()
        {
            
        }
    }
}