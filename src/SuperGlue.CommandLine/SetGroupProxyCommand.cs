using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue
{
    public class SetGroupProxyCommand : ICommand
    {
        public string Group { get; set; }
        public int Port { get; set; }

        public async Task Execute()
        {
            var basePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            var configPath = string.Format("{0}/applications.config", basePath.AbsolutePath);

            var configuration = await File
                .Open(configPath, FileMode.OpenOrCreate)
                .ReadAsJson<ApplicationsConfiguration>() ?? new ApplicationsConfiguration();

            configuration.SetProxyForGroup(Group, Port);

            await Files.WriteJsonTo(configPath, configuration);
        }
    }
}