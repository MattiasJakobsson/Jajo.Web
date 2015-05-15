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
    public class RunCommand : ICommand
    {
        public string AppPath { get; set; }

        public void Execute()
        {
            var privateBinPath = new StringBuilder();
            privateBinPath.Append(AppPath);

            if (Directory.Exists(Path.Combine(AppPath, "bin")))
                privateBinPath.AppendFormat(";{0}", Path.Combine(AppPath, "bin"));

            var applicationDomain = AppDomain.CreateDomain("SuperGlue", null, new AppDomainSetup
            {
                PrivateBinPath = privateBinPath.ToString(),
                ApplicationBase = AppPath,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            });

            var proxy = (AssemblyProxy)applicationDomain.CreateInstanceAndUnwrap(typeof(AssemblyProxy).Assembly.FullName, typeof(AssemblyProxy).FullName);

            proxy.LoadAssembly(typeof(RemoteHost).Assembly.Location);
            proxy.LoadAssembly(typeof(ErrorPageOptions).Assembly.Location);
            proxy.LoadAssembly(typeof(OwinHttpListener).Assembly.Location);
            proxy.LoadAssembly(typeof(WebApp).Assembly.Location);

            var katanaRunner = (RemoteHost)applicationDomain.CreateInstanceAndUnwrap(typeof(RemoteHost).Assembly.FullName, typeof(RemoteHost).FullName);

            katanaRunner.Start();

            Console.ReadLine();

            katanaRunner.Stop();
        }
    }
}