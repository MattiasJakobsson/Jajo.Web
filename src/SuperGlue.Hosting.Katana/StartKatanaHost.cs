using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using SuperGlue.Configuration;

namespace SuperGlue.Hosting.Katana
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartKatanaHost : IStartApplication
    {
        private IDisposable _webApp;

        public string Chain { get { return "chains.Web"; } }

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            return Task.Factory.StartNew(() =>
            {
                var url = settings.Get("superglue.Web.Urls", "http://localhost:8020");

                _webApp = WebApp.Start(new StartOptions(url), x => x.Use<RunAppFunc>(new RunAppFuncOptions(chain)));
            });
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            return Task.Factory.StartNew(() =>
            {
                if (_webApp != null)
                    _webApp.Dispose();
            });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return null;
        }
    }
}