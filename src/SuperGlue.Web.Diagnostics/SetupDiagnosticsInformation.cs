using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.Diagnostics
{
    public class SetupDiagnosticsInformation : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.DiagnosticsWebSetup", environment =>
            {
                environment.AlterSettings<RouteSettings>(x => x.UsePolicy(new DiagnosticsRoutePolicy()));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}