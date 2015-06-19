using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public class SetupAssetConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup()
        {
            yield return new ConfigurationSetupResult("superglue.AssetsSetup", environment =>
            {
                environment.AlterSettings<OutputSettings>(x => x.When(y => (y.GetOutput() as IContainAssetInformation) != null).UseRenderer(new RenderAssetOutput()));
            }, "superglue.OutputSetup");

            yield return new ConfigurationSetupResult("superglue.AssetsRoutingSetup", environment =>
            {
                environment.CreateRoute("/_assets/{AssetType}/{*AssetPath}", typeof(AssetEndpoint).GetMethod("Asset", new[] { typeof(AssetEndpointInput) }));
            }, "superglue.RoutingSetup");
        }

        public void Shutdown(IDictionary<string, object> applicationData)
        {

        }

        public void Configure(SettingsConfiguration configuration)
        {

        }
    }
}