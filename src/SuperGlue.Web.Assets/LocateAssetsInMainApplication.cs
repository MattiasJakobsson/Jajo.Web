using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Assets
{
    public class LocateAssetsInMainApplication : ILocateAssets
    {
        public Task<IEnumerable<Asset>> FindAssets(IDictionary<string, object> environment)
        {
            var assetDirectory = environment.ResolvePath("~/assets/");

            return Task.Factory.StartNew(() => LoadFromDirectory(assetDirectory, assetDirectory));
        }

        private static IEnumerable<Asset> LoadFromDirectory(string baseDirectory, string directory)
        {
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                var name = file.Replace(baseDirectory, "");

                yield return new Asset(name, File.OpenRead(file), 1);
            }

            var childDirectories = Directory.GetDirectories(directory);

            foreach (var asset in childDirectories.SelectMany(childDirectory => LoadFromDirectory(baseDirectory, childDirectory)))
                yield return asset;
        }
    }
}