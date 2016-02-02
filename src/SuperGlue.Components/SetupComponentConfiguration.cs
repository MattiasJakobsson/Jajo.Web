using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Components
{
    public class SetupComponentConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ComponentsSetup", environment =>
            {
                environment.RegisterAll(typeof(IComponentSource));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}