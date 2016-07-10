using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Web.Endpoints
{
    public class ConfigureEndpoints : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EndpointsSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.ScanOpenType(typeof(IExecuteTypeOfEndpoint<>)).Register(typeof(IExecuteAnyEndpoint), typeof(DefaultEndpointExecutor)));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}