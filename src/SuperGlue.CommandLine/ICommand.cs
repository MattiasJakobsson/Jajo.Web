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
    public interface ICommand
    {
        void Execute();
    }

    public class RunCommand : ICommand
    {
        public void Execute()
        {
            var privateBinPath = new StringBuilder();
            privateBinPath.Append(Environment.CurrentDirectory);

            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "bin")))
                privateBinPath.AppendFormat(";{0}", Path.Combine(Environment.CurrentDirectory, "bin"));

            var applicationDomain = AppDomain.CreateDomain("SuperGlue", null, new AppDomainSetup
            {
                PrivateBinPath = privateBinPath.ToString(),
                ApplicationBase = Environment.CurrentDirectory,
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