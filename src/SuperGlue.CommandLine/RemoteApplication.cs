using System;
using System.IO;
using System.Text;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Hosting;
using SuperGlue.Configuration;
using SuperGlue.Hosting.Katana;

namespace SuperGlue
{
    public class RemoteApplication
    {
        private readonly string _path;
        private AppDomain _appDomain;
        private RemoteHost _host;

        public RemoteApplication(string path)
        {
            _path = path;
        }

        public void Start()
        {
            var privateBinPath = new StringBuilder();
            privateBinPath.Append(_path);

            if (Directory.Exists(Path.Combine(_path, "bin")))
                privateBinPath.AppendFormat(";{0}", Path.Combine(_path, "bin"));

            _appDomain = AppDomain.CreateDomain("SuperGlue", null, new AppDomainSetup
            {
                PrivateBinPath = privateBinPath.ToString(),
                ApplicationBase = _path,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            });

            var proxy = (AssemblyProxy)_appDomain.CreateInstanceAndUnwrap(typeof(AssemblyProxy).Assembly.FullName, typeof(AssemblyProxy).FullName);

            proxy.LoadAssembly(typeof(RemoteHost).Assembly.Location);
            proxy.LoadAssembly(typeof(ErrorPageOptions).Assembly.Location);
            proxy.LoadAssembly(typeof(OwinHttpListener).Assembly.Location);
            proxy.LoadAssembly(typeof(WebApp).Assembly.Location);

            _host = (RemoteHost)_appDomain.CreateInstanceAndUnwrap(typeof(RemoteHost).Assembly.FullName, typeof(RemoteHost).FullName);

            _host.Start();
        }

        public void Stop()
        {
            _host.Stop();

            AppDomain.Unload(_appDomain);
        }

        public void Recycle()
        {
            Stop();
            Start();
        }
    }
}