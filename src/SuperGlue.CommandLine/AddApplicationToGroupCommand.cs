using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue
{
    public class AddApplicationToGroupCommand : ICommand
    {
        public string Group { get; set; }
        public string AppPath { get; set; }

        public async Task Execute()
        {
            var basePath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            var configPath = string.Format("{0}/applications.config", basePath.AbsolutePath);

            var configuration = await File
                .Open(configPath, FileMode.OpenOrCreate)
                .ReadAsJson<ApplicationsConfiguration>() ?? new ApplicationsConfiguration();

            configuration.AddApplicationTo(Group, AppPath);

            await Files.WriteJsonTo(configPath, configuration);
        }
    }
}