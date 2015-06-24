using System.IO;
using SuperGlue.Configuration;
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

            var subApplications = SubApplications.Init(AppPath);

            var priority = 2;

            foreach (var subApplication in subApplications)
            {
                var application = subApplication;
                var currentPriority = priority;

                settings.AddSource(Path.Combine(application.Path, "/assets"), currentPriority);

                priority++;
            }

            Assets.CollectAllAssets(settings).Wait();
        }
    }
}