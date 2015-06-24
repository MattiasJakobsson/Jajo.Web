using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Assets
{
    public class DefaultAssetsMover : IMoveAssetsToCorrectLocation
    {
        public async Task Move(IEnumerable<Asset> assets, IDictionary<string, object> environment)
        {
            var assetsToUse = assets
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToList();

            var baseDirectory = environment.ResolvePath("~/_assets");

            foreach (var asset in assetsToUse)
            {
                var assetPath = Path.Combine(baseDirectory, asset.Name);
                var assetDirectory = Path.GetDirectoryName(assetPath);

                if(string.IsNullOrEmpty(assetDirectory))
                    continue;

                if (!Directory.Exists(assetDirectory))
                    Directory.CreateDirectory(assetDirectory);

                await asset.Content.CopyToAsync(File.Create(assetPath));
            }
        }
    }
}