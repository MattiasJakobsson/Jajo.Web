using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.ApiDiscovery;
using SuperGlue.Configuration;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.ApiDiscovery
{
    public class ConfigureWebApiDiscovery : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.WebApiDiscoverySetup", environment =>
            {
                environment.RegisterAll(typeof(IRenderApiOutput));
                environment.RegisterAll(typeof(IRootApiInput));

                var apiRenderers = environment.ResolveAll<IRenderApiOutput>();

                foreach (var apiRenderer in apiRenderers)
                {
                    var currentRenderer = apiRenderer;

                    environment.AlterSettings<OutputSettings>(x => x
                        .When(y => (y.GetOutput() as IApiResponse) != null && y.GetRequest().Headers.Accept.Contains(currentRenderer.Type))
                        .UseRenderer(currentRenderer));
                }

                return Task.CompletedTask;
            }, "superglue.OutputSetup");
        }
    }
}