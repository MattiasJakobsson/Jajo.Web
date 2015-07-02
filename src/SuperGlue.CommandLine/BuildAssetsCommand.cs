using System.IO;
using SuperGlue.Web.Assets;

namespace SuperGlue
{
    public class BuildAssetsCommand : ICommand
    {
        public string AppPath { get; set; }
        public string Destination { get; set; }

        public void Execute()
        {
            //HACK:Hard coded path to assets
            var settings = new AssetSettings()
                .SetSetupEnabled(true)
                .UseDestination(Destination)
                .AddSource(Path.Combine(AppPath, "assets"), 1);

            Assets.CollectAllAssets(settings).Wait();
        }
    }
}