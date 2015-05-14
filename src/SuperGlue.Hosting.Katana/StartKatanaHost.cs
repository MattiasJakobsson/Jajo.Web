using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Katana
{
    public class StartKatanaHost : IStartApplication
    {
        private IDisposable _webApp;

        public void Start(IDictionary<string, Func<IDictionary<string, object>, Task>> chains, IDictionary<string, object> environment)
        {
            if(!chains.ContainsKey("chains.Web"))
                return;

            var url = environment.Get("superglue.Web.Urls", "http://localhost:8020");

            var webChain = chains["chains.Web"];

            _webApp = WebApp.Start(url, x => x.Use<RunAppFunc>(new RunAppFuncOptions(webChain, environment)));
        }

        public void ShutDown()
        {
            if(_webApp != null)
                _webApp.Dispose();
        }
    }
}