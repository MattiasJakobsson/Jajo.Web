using System.Collections.Generic;
using System.Threading.Tasks;
using StructureMap;
using SuperGlue.Configuration;

namespace SuperGlue.StructureMap
{
    public class SetupStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            var container = new Container();

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x => x[StructureMapEnvironmentExtensions.StructureMapConstants.Container] = container);
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment =>
            {
                environment.SetupContainerInEnvironment(container);
            }, "superglue.StructureMap.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() =>
            {
                applicationData.GetContainer().Dispose();
            });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}