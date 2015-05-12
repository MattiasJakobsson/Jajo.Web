namespace Jajo.Web.Assets
{
    public interface IAssetPipeline
    {
        string GenerateUrlForFiles(params string[] files);
    }

    public class AssetPipeline : IAssetPipeline
    {
        public string GenerateUrlForFiles(params string[] files)
        {
            throw new System.NotImplementedException();
        }
    }
}