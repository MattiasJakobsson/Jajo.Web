using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Assets
{
    public class LocateAssetsInSubApplications : ILocateAssets
    {
        public Task<IEnumerable<Asset>> FindAssets(IDictionary<string, object> environment)
        {
            var subApplications = environment.Get<IEnumerable<InitializedSubApplication>>(SubApplicationsEnvironmentExtensions.SubApplicationConstants.SubApplications);

            return Task.Factory.StartNew(() => LoadFromSubApplications(subApplications));
        }

        private static IEnumerable<Asset> LoadFromSubApplications(IEnumerable<InitializedSubApplication> subApplications)
        {
            return subApplications.Select(subApplication => Path.Combine(subApplication.Path, "/assets")).SelectMany(assetPath => LoadFromDirectory(assetPath, assetPath));
        }

        private static IEnumerable<Asset> LoadFromDirectory(string baseDirectory, string directory)
        {
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                var name = file.Replace(baseDirectory, "");

                yield return new Asset(name, File.OpenRead(file), 2);
            }

            var childDirectories = Directory.GetDirectories(directory);

            foreach (var asset in childDirectories.SelectMany(childDirectory => LoadFromDirectory(baseDirectory, childDirectory)))
                yield return asset;
        }

    }
}