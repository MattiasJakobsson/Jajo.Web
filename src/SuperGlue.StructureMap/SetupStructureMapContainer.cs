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

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x =>
            {
                x[StructureMapEnvironmentExtensions.StructureMapConstants.Container] = container;

                return Task.CompletedTask;
            });

            yield return new ConfigurationSetupResult("superglue.ContainerSetup", environment =>
            {
                environment.SetupContainerInEnvironment(container);

                return Task.CompletedTask;
            }, "superglue.StructureMap.ContainerSetup", environment =>
            {
                environment.GetContainer().Dispose();

                return Task.CompletedTask;
            });
        }
    }
}