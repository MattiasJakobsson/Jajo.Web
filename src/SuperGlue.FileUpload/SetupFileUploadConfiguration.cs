using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.FileUpload
{
    public class SetupFileUploadConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FileUploadSetup", environment =>
            {
                environment.RegisterTransient(typeof(IUploadFiles), typeof(DefaultFileUploader));

                environment.AlterSettings<FileUploadSettings>(x => x.UploadTo("/Upload"));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}