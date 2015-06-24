using System.Collections.Generic;
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
                environment.RegisterAll(typeof(ILocateAssets));
                environment.RegisterTransient(typeof(IMoveAssetsToCorrectLocation), typeof(DefaultAssetsMover));

                environment.AlterSettings<AssetSettings>(x => x.AssetsSetupEnabled = applicationEnvironment == "test");
            }, "superglue.ContainerSetup");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            return Task.Factory.StartNew(() => { });
        }

        public async Task Configure(SettingsConfiguration configuration)
        {
            if (configuration.WithSettings<AssetSettings>().AssetsSetupEnabled)
            {
                var assetLocators = configuration.Settings.ResolveAll<ILocateAssets>();

                var assets = new List<Asset>();

                foreach (var assetLocator in assetLocators)
                    assets.AddRange(await assetLocator.FindAssets(configuration.Settings));

                var assetMover = configuration.Settings.Resolve<IMoveAssetsToCorrectLocation>();

                await assetMover.Move(assets, configuration.Settings);
            }
        }
    }
}