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

            var shouldStartProxy = false;
            var proxyPort = 8800;
            string name;

            var group = configuration.Groups.FirstOrDefault(x => x.Name == Application);

            var runners = new List<RunnableApplication>();

            if (group != null)
            {
                shouldStartProxy = group.UseProxy;
                proxyPort = group.ProxyPort;
                name = group.Name;

                foreach (var application in group.Applications)
                {
                    var details = group.GetApplicationDetails(application.Path, basePath.AbsolutePath);

                    var runner = ApplicationRunnerFactories
                        .Where(x => x.Item1(details.Path))
                        .Select(x => x.Item2(details.Destination))
                        .FirstOrDefault(x => x != null);

                    if (runner == null)
                        continue;

                    runners.Add(new RunnableApplication(runner, details, Environment));
                }
            }
            else
            {
                configuration.AddApplication(Application);

                var details = configuration.GetApplicationDetails(Application, basePath.AbsolutePath);

                name = details.Name;

                var runner = ApplicationRunnerFactories
                    .Where(x => x.Item1(details.Path))
                    .Select(x => x.Item2(details.Path))
                    .FirstOrDefault(x => x != null);

                if (runner != null)
                    runners.Add(new RunnableApplication(runner, details, Environment));
            }

            await Files.WriteJsonTo(configPath, configuration);

            await Task.WhenAll(runners.Select(x => x.Start(basePath.AbsolutePath)));

            NginxProxy proxy = null;

            if (shouldStartProxy)
            {
                proxy = new NginxProxy(runners.Select(x => x.GetApplicationPath()), proxyPort, name);

                proxy.Start();
            }

            var key = Console.ReadKey();

            while (key.Key != ConsoleKey.Q)
            {
                if (key.Key != ConsoleKey.R)
                    continue;

                Console.WriteLine();

                await Task.WhenAll(runners.Select(x => x.Recycle()));

                key = Console.ReadKey();
            }

            await Task.WhenAll(runners.Select(x => x.Stop()));

            if (proxy != null)
                proxy.Stop();
        }
    }
}