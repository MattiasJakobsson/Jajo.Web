using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.FileUpload
{
    public class SetupFileUploadConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FileUploadSetup", environment =>
            {
                environment.RegisterTransient(typeof(IPersistFiles), typeof(FileSystemFilePersister));
                environment.RegisterAll(typeof(ITransformFile));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}