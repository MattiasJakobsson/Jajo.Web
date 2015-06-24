using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Assets
{
    public static class Assets
    {
        public static async Task CollectAllAssets(AssetSettings settings)
        {
            var assets = new List<Asset>();

            foreach (var source in settings.Sources)
                assets.AddRange(LoadFromDirectory(source.Path, source.Path, source.Priority));

            await Move(assets, settings.AssetsDestination);
        }

        private static async Task Move(IEnumerable<Asset> assets, string moveTo)
        {
            var assetsToUse = assets
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToList();

            foreach (var asset in assetsToUse)
            {
                var assetPath = Path.Combine(moveTo, asset.Name);
                var assetDirectory = Path.GetDirectoryName(assetPath);

                if (string.IsNullOrEmpty(assetDirectory))
                    continue;

                if (!Directory.Exists(assetDirectory))
                    Directory.CreateDirectory(assetDirectory);

                await asset.Content.CopyToAsync(File.Create(assetPath));
            }
        }

        private static IEnumerable<Asset> LoadFromDirectory(string baseDirectory, string directory, int priority)
        {
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                var name = file.Replace(baseDirectory, "");

                yield return new Asset(name, File.OpenRead(file), priority);
            }

            var childDirectories = Directory.GetDirectories(directory);

            foreach (var asset in childDirectories.SelectMany(childDirectory => LoadFromDirectory(baseDirectory, childDirectory, priority)))
                yield return asset;
        }
    }
}