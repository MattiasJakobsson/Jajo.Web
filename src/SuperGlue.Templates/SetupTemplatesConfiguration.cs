using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Templates
{
    public class SetupTemplatesConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.TemplatesSetup", environment =>
            {
                environment.RegisterAll(typeof(ITemplateSource));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}