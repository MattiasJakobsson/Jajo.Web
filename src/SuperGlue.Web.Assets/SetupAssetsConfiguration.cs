using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Assets
{
    public class SetupAssetsConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.AssetsSetup", environment =>
            {
                environment.RegisterTransient(typeof(IAssetRequirements), typeof(DefaultAssetRequirements));
                environment.RegisterTransient(typeof(IAssetTagWriter), typeof(DefaultAssetTagWriter));

                //HACK:Hard coded path to assets
                environment.AlterSettings<AssetSettings>(x => x
                    .SetSetupEnabled(applicationEnvironment == "test")
                    .UseDestination(environment.ResolvePath("~/_assets"))
                    .AddSource(environment.ResolvePath("~/assets"), 1));

                var subApplications = environment.Get<IEnumerable<InitializedSubApplication>>(SubApplicationsEnvironmentExtensions.SubApplicationConstants.SubApplications);

                var priority = 2;
                foreach (var subApplication in subApplications)
                {
                    var application = subApplication;
                    var currentPriority = priority;

                    environment.AlterSettings<AssetSettings>(x => x.AddSource(Path.Combine(application.Path, "/assets"), currentPriority));

                    priority++;
                }

            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public async Task Configure(SettingsConfiguration configuration)
        {
            await Assets.CollectAllAssets(configuration.WithSettings<AssetSettings>());
        }
    }
}