using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.EventTracking
{
    public class SetupEventTrackingConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EventTrackingSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(IAmInterestedInEventAwareItems)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}