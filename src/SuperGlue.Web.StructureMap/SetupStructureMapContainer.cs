using System.Collections.Generic;
using StructureMap;
using SuperGlue.Web.Configuration;

namespace SuperGlue.Web.StructureMap
{
    public class SetupStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            var container = new Container();

            yield return new ConfigurationSetupResult("superglue.StructureMap.ContainerSetup", x => x["superglue.StructureMap.Container"] = container);
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}