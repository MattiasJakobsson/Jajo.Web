using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Configuration
{
    public static class SubApplications
    {
        private static readonly ICollection<InitializedSubApplication> InitializedApplications = new List<InitializedSubApplication>();

        public static IEnumerable<InitializedSubApplication> Init(string basePath)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

            if (!basePath.EndsWith("\\"))
                basePath = string.Format("{0}\\", basePath);

            var subAppPathsConfigurations = ConfigurationFile.Read<SubAppsConfiguration>(string.Format("{0}.subapplications", basePath));

            var subAppsDirectory = string.Format("{0}SubApplications\\", basePath);

            var applications = Directory.Exists(subAppsDirectory) ? Directory.GetDirectories(subAppsDirectory) : new string[0];

            InitializedApplications.Clear();

            var subApplicationPaths = new List<SubApplication>();

            subApplicationPaths.AddRange(subAppPathsConfigurations.SubApplications);
            subApplicationPaths.AddRange(applications.Select(x => new SubApplication
            {
                Name = new DirectoryInfo(x).Name,
                AbsolutePath = x
            }).Where(x => subAppPathsConfigurations.SubApplications.All(y => y.Name != x.Name)));

            var shadowCopyPath = string.Format("{0}Runtime\\SubApplications\\", basePath);

            if(Directory.Exists(shadowCopyPath))
                Directory.Delete(shadowCopyPath, true);

            Directory.CreateDirectory(shadowCopyPath);

            var result = new List<InitializedSubApplication>();

            foreach (var originalPath in subApplicationPaths)
            {
                var subApplicationPath = string.Format("{0}{1}\\", shadowCopyPath, originalPath.Name);

                if (!Directory.Exists(subApplicationPath))
                    Directory.CreateDirectory(subApplicationPath);

                DirectoryCopy(originalPath.GetAbsolutePath(basePath), subApplicationPath, true);

                var assembliesPaths = Directory.GetFiles(subApplicationPath, "*.dll");

                foreach (var assemblyPath in assembliesPaths)
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName == AssemblyName.GetAssemblyName(assemblyPath).FullName || AssemblyName.GetAssemblyName(assemblyPath).FullName.Split(',')[0] == x.FullName.Split(',')[0]))
                        continue;

                    var assembly = Assembly.LoadFile(Path.GetFullPath(assemblyPath));

                    var application = new InitializedSubApplication(subApplicationPath, assembly.FullName, assembly);
                    InitializedApplications.Add(application);

                    result.Add(application);
                }
            }

            return result;
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pluginsFolders = InitializedApplications.Select(x => new DirectoryInfo(x.Path));

            return (from f in pluginsFolders.SelectMany(x => x.GetFiles("*.dll", SearchOption.AllDirectories))
                    let assemblyName = AssemblyName.GetAssemblyName(f.FullName)
                    where (assemblyName.FullName == args.Name || assemblyName.FullName.Split(',')[0] == args.Name) 
                        && InitializedApplications.Any(x => x.Assembly.FullName == args.Name || x.Assembly.FullName.Split(',')[0] == args.Name.Split(',')[0])
                    select Assembly.LoadFile(f.FullName)).FirstOrDefault();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            var dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (!copySubDirs) return;
            
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true);
            }
        }
    }
}