using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Configuration
{
    public class SetupMiddlewareWrappersConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.MiddlewareWrappersSetup", environment =>
                {
                    environment.AlterSettings<IocConfiguration>(x => x.ScanOpenType(typeof(IWrapMiddleware<>)));

                    return Task.CompletedTask;
                }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.CompletedTask;
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            return Task.CompletedTask;
        }
    }
}