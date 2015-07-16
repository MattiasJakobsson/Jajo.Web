using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Owin;
using SuperGlue.Configuration;
using SuperGlue.Logging;

namespace SuperGlue.Hosting.Katana
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StartKatanaHost : IStartApplication
    {
        private IDisposable _webApp;
        private readonly ILog _log;

        public StartKatanaHost(ILog log)
        {
            _log = log;
        }

        public string Chain { get { return "chains.Web"; } }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            _log.Info("Starting katana host for environment: \"{0}\"", environment);

            var katanaSettings = settings.GetSettings<KatanaSettings>();
            var bindings = katanaSettings.GetBindings();

            var startOptions = new StartOptions();

            foreach (var binding in bindings)
                startOptions.Urls.Add(binding);

            var optionsPath = settings.ResolvePath("~/.hostingoptions");

            var options = await Files.ReadAsJson<HostingOptions>(optionsPath) ?? new HostingOptions();

            if (startOptions.Urls.Any())
                options.Bindings = startOptions.Urls;

            if (!options.Bindings.Any())
                options.Bindings.Add(string.Format("http://localhost:{0}", GetRandomUnusedPort()));

            options.ApplicationName = settings.GetApplicationName();
            options.ApplicationBasePath = settings.GetWebApplicationRoot();

            await Files.WriteJsonTo(optionsPath, options);

            foreach (var binding in options.Bindings)
                startOptions.Urls.Add(binding);

            _webApp = WebApp.Start(startOptions, x => x.Use<RunAppFunc>(new RunAppFuncOptions(chain)));

            _log.Info("Katana host started with bindings: {0}", string.Join(", ", startOptions.Urls));
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            if (_webApp != null)
                _webApp.Dispose();

            return Task.CompletedTask;
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return null;
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}