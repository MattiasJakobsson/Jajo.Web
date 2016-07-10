using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Web.RouteInputValidator
{
    public class SetupRouteInputValidatorConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RouteInputValidatorSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.ScanOpenType(typeof(IEnsureExists<>)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}