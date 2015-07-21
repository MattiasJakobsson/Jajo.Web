using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue
{
    public class RunCommand : ICommand
    {
        private static readonly IEnumerable<Tuple<Func<string, bool>, Func<string, IApplicationRunner>>> ApplicationRunnerFactories = new List<Tuple<Func<string, bool>, Func<string, IApplicationRunner>>>
        {
            new Tuple<Func<string, bool>, Func<string, IApplicationRunner>>(TopshelfHostApplicationRunner.CanRun, x => new TopshelfHostApplicationRunner(x))
        };

        public string Application { get; set; }
        public string Environment { get; set; }

        public async Task Execute()
        {
            var basePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            var configPath = string.Format("{0}/applications.config", basePath.AbsolutePath);

            var configuration = await File
                .Open(configPath, FileMode.OpenOrCreate)
                .ReadAsJson<ApplicationsConfiguration>() ?? new ApplicationsConfiguration();

            var applicationPathsToRun = new List<Tuple<string, string>>();
            var shouldStartProxy = false;
            var proxyPort = 8800;
            var name = Guid.NewGuid().ToString();

            var group = configuration.Groups.FirstOrDefault(x => x.Name == Application);

            //TODO:Run config transformations
            //TODO:Setup watchers that recycles when something changes

            if (group != null)
            {
                shouldStartProxy = group.UseProxy;
                proxyPort = group.ProxyPort;
                name = group.Name;

                var groupDirectory = string.Format("{0}/{1}", basePath.AbsolutePath, group.Name);

                if(Directory.Exists(groupDirectory))
                    new DirectoryInfo(groupDirectory).DeleteDirectoryAndChildren();

                foreach (var application in group.Applications)
                {
                    var destination = string.Format("{0}/{1}", groupDirectory, Guid.NewGuid());
                    applicationPathsToRun.Add(new Tuple<string, string>(application, destination));
                    
                    DirectoryCopy(application, destination, true);
                }
            }
            else
            {
                var destination = string.Format("{0}/applications/{1}", basePath.AbsolutePath, name);
                applicationPathsToRun.Add(new Tuple<string, string>(Application, destination));

                DirectoryCopy(Application, destination, true);
            }

            var applicationRunners = applicationPathsToRun
                .Select(x => new
                {
                    Paths = x,
                    RunnerFactory = ApplicationRunnerFactories.FirstOrDefault(y => y.Item1(x.Item2))
                })
                .Where(x => x.RunnerFactory != null)
                .Select(x => x.RunnerFactory.Item2(x.Paths.Item2))
                .Where(x => x != null)
                .ToList();

            var startActions = applicationRunners.Select(x => x.Start(Environment));

            await Task.WhenAll(startActions);

            NginxProxy proxy = null;

            if (shouldStartProxy)
            {
                proxy = new NginxProxy(applicationPathsToRun.Select(x => x.Item2), proxyPort, name);

                proxy.Start();
            }

            var key = Console.ReadKey();

            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key != ConsoleKey.R)
                    continue;

                Console.WriteLine();

                await Task.WhenAll(applicationRunners.Select(x => x.Recycle(Environment)));

                key = Console.ReadKey();
            }

            await Task.WhenAll(applicationRunners.Select(x => x.Stop()));

            if (proxy != null)
                proxy.Stop();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (!copySubDirs)
                return;
            
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, true);
            }
        }
    }
}