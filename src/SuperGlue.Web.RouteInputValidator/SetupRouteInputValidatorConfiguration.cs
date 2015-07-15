using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.RouteInputValidator
{
    public class SetupRouteInputValidatorConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.RouteInputValidatorSetup", environment =>
            {
                environment.RegisterAllClosing(typeof(IEnsureExists<>));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}