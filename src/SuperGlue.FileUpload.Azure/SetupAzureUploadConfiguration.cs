using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.FileUpload.Azure
{
    public class SetupAzureUploadConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.AzureUploadSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IUploadFiles), typeof(UploadFilesToAzureBlobStorage)));

                environment.AlterSettings<AzureUploadSettings>(x => x.UseStorageAccount(environment.Resolve<IApplicationConfiguration>().GetConnectionString("Azure.Storage")).GetContainerNameUsing(y => "upload"));

                return Task.CompletedTask;
            }, "superglue.FileUploadSetup");
        }
    }
}