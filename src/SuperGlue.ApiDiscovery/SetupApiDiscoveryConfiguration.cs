using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ApiDiscovery
{
    public class SetupApiDiscoveryConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ApiDiscoverySetup", environment =>
            {
                environment.RegisterAll(typeof(IParseApiResponse));
                environment.RegisterTransient(typeof(IExecuteApiRequests), typeof(DefaultApiRequestExecutor));
                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}