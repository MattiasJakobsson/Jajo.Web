using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Web.Validation
{
    public class SetupValidationConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ValidationSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(IValidateRequest)));

                return Task.CompletedTask;
            }, "superglue.Container");
        }
    }
}