using System.Collections.Generic;
using SuperGlue.Web.Configuration;
using SuperGlue.Web.Diagnostics;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.StructureMap;
using SuperGlue.Web.UnitOfWork;

namespace SuperGlue.Web.Sample
{
    public class ConfigureStructureMapContainer : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("StructureMap.Configured", applicationData =>
            {
                var container = applicationData.GetContainer();
                var assemblies = applicationData.GetAssemblies();

                var diagnosticsManager = new ManageDiagnosticsInformationInMemory();

                container.Configure(x =>
                {
                    x.Scan(y =>
                    {
                        y.AssemblyContainingType(typeof(IExecuteTypeOfEndpoint<>));

                        foreach (var assembly in assemblies)
                            y.Assembly(assembly);

                        y.ConnectImplementationsToTypesClosing(typeof(IExecuteTypeOfEndpoint<>));
                        y.AddAllTypesOf<ISuperGlueUnitOfWork>();
                        y.AddAllTypesOf<ISetupConfigurations>();
                    });

                    x.For<IManageDiagnosticsInformation>().Use(diagnosticsManager);
                });
            });
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {
            
        }
    }
}