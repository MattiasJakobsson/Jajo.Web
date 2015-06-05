using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Endpoints
{
    public class ConfigureEndpoints : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.UnitOfWork.Configure", environment =>
            {
                environment.RegisterAllClosing(typeof(IExecuteTypeOfEndpoint<>));

                environment.RegisterTransient(typeof(IExecuteAnyEndpoint), typeof(DefaultEndpointExecutor));
            }, "superglue.ContainerSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }

        public void Configure(SettingsConfiguration configuration)
        {

        }
    }
}