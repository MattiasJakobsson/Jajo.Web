using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.FileSystem
{
    public class SetupFileSystemConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FileSystemSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IFileSystem), typeof(FileSystem)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}