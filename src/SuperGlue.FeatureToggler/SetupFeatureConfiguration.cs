using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.FeatureToggler
{
    public class SetupFeatureConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FeaturesSetup", environment =>
            {
                environment.RegisterTransient(typeof(ICheckIfFeatureIsEnabled), typeof(DefaultFeatureEnabledChecker));
                environment.RegisterAll(typeof(IFeature));
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.CompletedTask;
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            configuration.Settings[FeatureEnvironmentExtensions.FeatureConstants.FeatureSettings] = configuration.WithSettings<FeatureSettings>();

            return Task.CompletedTask;
        }
    }
}