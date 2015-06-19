using System.Collections.Generic;
using System.IO;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Assets
{
    public class AssetEndpoint
    {
        private readonly IDictionary<string, object> _environment;

        public AssetEndpoint(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public AssetEndpointResult Asset(AssetEndpointInput input)
        {
            //TODO:Make configurable
            var filePath = _environment.ResolvePath(string.Format("~/assets/{0}/{1}", input.AssetType, input.AssetPath));
        
            //TODO:Use correct mimetype
            return new AssetEndpointResult("", File.Open(filePath, FileMode.Open));
        }
    }

    public class AssetEndpointInput
    {
        public string AssetType { get; set; }
        public string AssetPath { get; set; }
    }

    public class AssetEndpointResult : IContainAssetInformation
    {
        public AssetEndpointResult(string mimeType, Stream asset)
        {
            MimeType = mimeType;
            Asset = asset;
        }

        public string MimeType { get; private set; }
        public Stream Asset { get; private set; }
    }
}