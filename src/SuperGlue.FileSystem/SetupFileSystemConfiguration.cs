using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.FileSystem
{
    public class SetupFileSystemConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FileSystemSetup", environment =>
            {
                environment.RegisterTransient(typeof(IFileSystem), typeof(FileSystem));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}