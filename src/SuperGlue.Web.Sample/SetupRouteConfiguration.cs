using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.Sample
{
    public class SetupRouteConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.WebSampleSetup", environment =>
            {
                environment.AlterSettings<RouteSettings>(x => x.UsePolicy(new QueryCommandMethodRoutePolicy(new List<Assembly> {GetType().Assembly})));

                return Task.CompletedTask;
            });
        }
    }
}