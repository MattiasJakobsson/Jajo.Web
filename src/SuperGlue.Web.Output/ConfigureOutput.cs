using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    public class ConfigureOutput : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.OutputSetup", environment =>
            {
                //TODO:Manage order somehow
                environment.AlterSettings<OutputSettings>(x => x
                    .When(y => y.GetRequest().Headers.Accept.Contains("application/json")).UseRenderer(new RenderOutputAsJson())
                    .When(y => y.GetRequest().Headers.Accept.Contains("application/xml")).UseRenderer(new RenderOutputAsXml())
                    .When(y => (y.GetOutput() as string) != null).UseRenderer(new RenderStringOutput())
                    .When(y => (y.GetOutput() as Stream) != null).UseRenderer(new RenderStreamOutput())
                    .When(y => (y.GetOutput() as IRedirectable) != null).UseRenderer(new RenderRedirectOutput()));

                environment.RegisterTransient(typeof(IRenderToOutput), typeof(DefaultOutputRenderer));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}