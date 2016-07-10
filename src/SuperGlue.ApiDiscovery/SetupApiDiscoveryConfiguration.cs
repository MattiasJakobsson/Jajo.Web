using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.ApiDiscovery
{
    public class SetupApiDiscoveryConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ApiDiscoverySetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(IParseApiResponse)).Register(typeof(IExecuteApiRequests), typeof(DefaultApiRequestExecutor)));
                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}