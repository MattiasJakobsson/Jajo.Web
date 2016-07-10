using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.FileUpload
{
    public class SetupFileUploadConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FileUploadSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IUploadFiles), typeof(DefaultFileUploader)));

                environment.AlterSettings<FileUploadSettings>(x => x.UploadTo("/Upload"));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}