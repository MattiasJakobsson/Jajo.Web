using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.EventTracking
{
    public class SetupEventTrackingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventTrackingSetup", environment =>
            {
                environment.RegisterAll(typeof(IAmInterestedInEventAwareItems));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}