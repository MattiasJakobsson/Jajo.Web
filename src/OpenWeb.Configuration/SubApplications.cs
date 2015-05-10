using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenWeb.Configuration
{
    public static class SubApplications
    {
        private static readonly List<string> SubApplicationPaths = new List<string>();

        public static IEnumerable<Assembly> Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

            var links = (File.Exists("subapps.txt") ? File.ReadAllText("subapps.txt") : "").Split('\n').Where(x => !string.IsNullOrEmpty(x)).ToList();

            var applications = Directory.GetDirectories("SubApplications\\");

            SubApplicationPaths.Clear();

            SubApplicationPaths.AddRange(links);
            SubApplicationPaths.AddRange(applications.Where(x => !links.Any(y => y.Contains(new DirectoryInfo(x).Name))));

            foreach (var subApplicationPath in SubApplicationPaths)
            {
                var assembliesPaths = Directory.GetFiles(subApplicationPath, "*.dll");

                foreach (var assemblyPath in assembliesPaths)
                {
                    if(AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName == AssemblyName.GetAssemblyName(assemblyPath).FullName))
                        continue;

                    var assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));

                    yield return assembly;
                }
            }
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pluginsFolders = SubApplicationPaths.Select(x =>new DirectoryInfo(Path.GetFullPath(x)));
            return (from f in pluginsFolders.SelectMany(x =>x.GetFiles("*.dll", SearchOption.AllDirectories))
                let assemblyName = AssemblyName.GetAssemblyName(f.FullName)
                where assemblyName.FullName == args.Name || assemblyName.FullName.Split(',')[0] == args.Name
                select Assembly.LoadFile(f.FullName)).FirstOrDefault();
        }
    }
}