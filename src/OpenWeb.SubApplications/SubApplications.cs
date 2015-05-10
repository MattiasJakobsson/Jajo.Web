using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenWeb.SubApplications
{
    public static class SubApplications
    {
        public static IEnumerable<Assembly> Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

            var subApplicationPaths = Directory.GetDirectories(string.Format("{0}SubApplications\\", AppDomain.CurrentDomain.SetupInformation.ApplicationBase));

            foreach (var subApplicationPath in subApplicationPaths)
            {
                var assembliesPaths = Directory.GetFiles(subApplicationPath, "*.dll");

                foreach (var assemblyPath in assembliesPaths)
                {
                    var assembly = Assembly.LoadFile(assemblyPath);

                    yield return assembly;
                }
            }
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pluginsFolder = new DirectoryInfo(string.Format("{0}SubApplications\\", AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
            return (from f in pluginsFolder.GetFiles("*.dll", SearchOption.AllDirectories)
                let assemblyName = AssemblyName.GetAssemblyName(f.FullName)
                where assemblyName.FullName == args.Name || assemblyName.FullName.Split(',')[0] == args.Name
                select Assembly.LoadFile(f.FullName)).FirstOrDefault();
        }
    }
}