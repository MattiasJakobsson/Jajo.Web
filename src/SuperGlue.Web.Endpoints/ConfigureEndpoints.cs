using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Endpoints
{
    public class ConfigureEndpoints : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.EndpointsSetup", environment =>
            {
                environment.RegisterAllClosing(typeof(IExecuteTypeOfEndpoint<>));

                environment.RegisterTransient(typeof(IExecuteAnyEndpoint), typeof(DefaultEndpointExecutor));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.Factory.StartNew(() => { });
        }
    }
}