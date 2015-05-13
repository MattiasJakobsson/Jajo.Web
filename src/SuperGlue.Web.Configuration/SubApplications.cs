using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Configuration
{
    public static class SubApplications
    {
        private static readonly List<string> SubApplicationPaths = new List<string>();
        private static readonly ICollection<InitializedSubApplication> InitializedApplications = new List<InitializedSubApplication>();

        public static IEnumerable<InitializedSubApplication> Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

            var links = (File.Exists("subapps.txt") ? File.ReadAllText("subapps.txt") : "").Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Replace("\r", "")).ToList();

            var applications = Directory.Exists("SubApplications\\") ? Directory.GetDirectories("SubApplications\\") : new string[0];

            SubApplicationPaths.Clear();
            InitializedApplications.Clear();

            SubApplicationPaths.AddRange(links);
            SubApplicationPaths.AddRange(applications.Where(x => !links.Any(y => y.Contains(new DirectoryInfo(x).Name))));

            foreach (var subApplicationPath in SubApplicationPaths)
            {
                var assembliesPaths = Directory.GetFiles(subApplicationPath, "*.dll");

                foreach (var assemblyPath in assembliesPaths)
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName == AssemblyName.GetAssemblyName(assemblyPath).FullName || AssemblyName.GetAssemblyName(assemblyPath).FullName.Split(',')[0] == x.FullName.Split(',')[0]))
                        continue;

                    var assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));

                    var application = new InitializedSubApplication(subApplicationPath, assembly.FullName, assembly);
                    InitializedApplications.Add(application);

                    yield return application;
                }
            }
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pluginsFolders = SubApplicationPaths.Select(x => new DirectoryInfo(Path.GetFullPath(x)));

            return (from f in pluginsFolders.SelectMany(x => x.GetFiles("*.dll", SearchOption.AllDirectories))
                    let assemblyName = AssemblyName.GetAssemblyName(f.FullName)
                    where (assemblyName.FullName == args.Name || assemblyName.FullName.Split(',')[0] == args.Name) && InitializedApplications.Any(x => x.Assembly.FullName == args.Name || x.Assembly.FullName.Split(',')[0] == args.Name.Split(',')[0])
                    select Assembly.LoadFile(f.FullName)).FirstOrDefault();
        }
    }
}