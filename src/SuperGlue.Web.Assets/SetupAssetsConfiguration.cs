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
                environment.RegisterTransient(typeof(IAssetRequirements), typeof(DefaultAssetRequirements));
                environment.RegisterTransient(typeof(IAssetTagWriter), typeof(DefaultAssetTagWriter));

                //HACK:Hard coded path to assets
                environment.AlterSettings<AssetSettings>(x => x
                    .SetSetupEnabled(applicationEnvironment == "test")
                    .UseDestination(environment.ResolvePath("~/_assets"))
                    .AddSource(environment.ResolvePath("~/assets"), 1));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}