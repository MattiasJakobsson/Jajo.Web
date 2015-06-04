using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    public class ConfigureOutput : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.OutputSetup", environment =>
            {
                environment.AlterSettings<OutputSettings>(x => x
                    .When(y => y.GetRequest().Headers.Accept.Contains("application/json")).UseRenderer(new RenderOutputAsJson())
                    .When(y => y.GetRequest().Headers.Accept.Contains("application/xml")).UseRenderer(new RenderOutputAsXml()));
            });
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }

        public void Configure(SettingsConfiguration configuration)
        {
            configuration.Environment[OutputEnvironmentExtensions.OutputConstants.Renderers] = configuration.WithSettings<OutputSettings>();
        }
    }
}