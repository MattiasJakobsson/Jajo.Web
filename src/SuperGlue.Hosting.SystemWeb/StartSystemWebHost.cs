using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.SystemWeb
{
    public class StartSystemWebHost : IStartApplication
    {
        public void Start(IDictionary<string, Func<IDictionary<string, object>, Task>> chains, IDictionary<string, object> environment)
        {
            if(!chains.ContainsKey("chains.Web"))
                return;

            var webChain = chains["chains.Web"];

            var appBuilder = environment.Get<IAppBuilder>(SystemWebEnvironmentConstants.AppBuilder);

            appBuilder.Use<RunAppFunc>(new RunAppFuncOptions(webChain, environment));
        }

        public void ShutDown()
        {
            
        }
    }
}