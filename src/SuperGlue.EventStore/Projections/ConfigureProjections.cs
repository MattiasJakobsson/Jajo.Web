using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.Projections
{
    public class ConfigureProjections : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventStore.Projections.Configured",
                environment =>
                {
                    environment.RegisterAll(typeof (IEventStoreProjection));

                    return Task.CompletedTask;
                }, 
                "superglue.ContainerSetup");
        }
    }
}