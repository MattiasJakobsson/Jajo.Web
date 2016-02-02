using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.StaticFiles
{
    public class SetupStaticFilesConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.StaticFilesSetup", environment =>
            {
                environment.AlterSettings<OutputSettings>(x => x.When(y => (y.GetOutput() as StaticFileOutput) != null).UseRenderer(new RenderStaticFileOutput()));

                return Task.CompletedTask;
            }, "superglue.OutputSetup");
        }
    }
}