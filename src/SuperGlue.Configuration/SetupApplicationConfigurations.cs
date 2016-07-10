using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Configuration
{
    public class SetupApplicationConfigurations : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.Configuration.ApplicationsConfigured", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Scan(typeof(IStartApplication))
                    .Scan(typeof(IConfigurationSource))
                    .Scan(typeof(IDefineChain))
                    .Register(typeof(IApplicationConfiguration), typeof(DefaultApplicationConfiguration)));

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