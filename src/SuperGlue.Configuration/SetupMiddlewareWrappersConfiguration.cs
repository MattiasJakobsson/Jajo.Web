using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class SetupMiddlewareWrappersConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.MiddlewareWrappersSetup", environment =>
                {
                    environment.RegisterAllClosing(typeof(IWrapMiddleware<>));
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