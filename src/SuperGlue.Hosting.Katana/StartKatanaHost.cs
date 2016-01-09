using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            settings.Log("Starting katana host for environment: \"{0}\"", LogLevel.Debug, environment);

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
                options.Bindings.Add($"http://localhost:{GetRandomUnusedPort()}");

            options.ApplicationName = settings.GetApplicationName();
            options.ApplicationBasePath = settings.GetWebApplicationRoot();

            await Files.WriteJsonTo(optionsPath, options);

            foreach (var binding in options.Bindings)
                startOptions.Urls.Add(binding);

            _webApp = WebApp.Start(startOptions, x => x.Use<RunAppFunc>(new RunAppFuncOptions(chain)));

            settings.Log("Katana host started with bindings: {0}", LogLevel.Debug, string.Join(", ", startOptions.Urls));
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