using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Configuration.Ioc;

namespace SuperGlue.Web.Assets
{
    public class SetupAssetsConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.AssetsSetup", environment =>
            {
                environment.AlterSettings<IocConfiguration>(x => x.Register(typeof(IAssetRequirements), typeof(DefaultAssetRequirements))
                    .Register(typeof(IAssetTagWriter), typeof(DefaultAssetTagWriter)));

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