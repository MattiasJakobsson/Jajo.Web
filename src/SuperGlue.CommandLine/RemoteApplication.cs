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
        private readonly string _environment;
        private AppDomain _appDomain;
        private RemoteKatanaHost _katanaHost;

        public RemoteApplication(string path, string environment)
        {
            _path = path;
            _environment = environment;
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

            Console.WriteLine("Starting application");

            var assemblyProxy = (AssemblyProxy)_appDomain.CreateInstanceAndUnwrap(typeof(AssemblyProxy).Assembly.FullName, typeof(AssemblyProxy).FullName);

            assemblyProxy.MakeSureAssemblyIsLoaded(typeof(RemoteKatanaHost).Assembly.Location);
            assemblyProxy.MakeSureAssemblyIsLoaded(typeof(ErrorPageOptions).Assembly.Location);
            assemblyProxy.MakeSureAssemblyIsLoaded(typeof(OwinHttpListener).Assembly.Location);
            assemblyProxy.MakeSureAssemblyIsLoaded(typeof(WebApp).Assembly.Location);

            _katanaHost = (RemoteKatanaHost)_appDomain.CreateInstanceAndUnwrap(typeof(RemoteKatanaHost).Assembly.FullName, typeof(RemoteKatanaHost).FullName);

            _katanaHost.Start(_environment);
        }

        public void Stop()
        {
            _katanaHost.Stop();

            AppDomain.Unload(_appDomain);
        }

        public void Recycle()
        {
            Stop();
            Start();
        }
    }
}