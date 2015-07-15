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

                return Task.CompletedTask;
            }, "superglue.ContainerSetup", configureAction: configuration =>
            {
                configuration.Settings[FeatureEnvironmentExtensions.FeatureConstants.FeatureSettings] = configuration.WithSettings<FeatureSettings>();

                return Task.CompletedTask;
            });
        }
    }
}