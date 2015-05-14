using System.Collections.Generic;
using StructureMap;
using SuperGlue.Configuration;

namespace SuperGlue.StructureMap
{
    public class SetupStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            var container = new Container();

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x => x[StructureMapEnvironmentExtensions.StructureMapConstants.Container] = container);
            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment =>
            {
                environment.SetupContainerInEnvironment(container);
            }, "superglue.StructureMap.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            applicationData.GetContainer().Dispose();
        }
    }
}