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

        public string Chain => "chains.Web";

        public Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Starting katana host for environment: \"{0}\"", LogLevel.Debug, environment);

            var katanaSettings = settings.GetSettings<KatanaSettings>();
            var bindings = katanaSettings.GetBindings();

            var startOptions = new StartOptions();

            foreach (var binding in bindings)
                startOptions.Urls.Add(binding);

            _webApp = WebApp.Start(startOptions, x => x.Use<RunAppFunc>(new RunAppFuncOptions(chain)));

            settings.Log("Katana host started with bindings: {0}", LogLevel.Debug, string.Join(", ", startOptions.Urls));

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            _webApp?.Dispose();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, IDictionary<string, object> settings, string environment)
        {
            return null;
        }
    }
}