using System.Collections.Generic;
using SuperGlue.Web.Configuration;
using SuperGlue.Web.Diagnostics;
using SuperGlue.Web.StructureMap;

namespace SuperGlue.Web.Sample
{
    public class ConfigureStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("StructureMap.Configured", applicationData =>
            {
                var container = applicationData.GetContainer();

                var diagnosticsManager = new ManageDiagnosticsInformationInMemory();

                container.Configure(x =>
                {
                    x.For<IManageDiagnosticsInformation>().Use(diagnosticsManager);
                });
            });
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}