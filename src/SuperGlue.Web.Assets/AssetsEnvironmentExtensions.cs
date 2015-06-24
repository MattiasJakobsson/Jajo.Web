using System.Collections.Generic;

namespace SuperGlue.Web.Assets
{
    public static class AssetsEnvironmentExtensions
    {
        public static class AssetsConstants
        {
            public const string AssetSettings = "superglue.AssetSettings";
        }

        public static AssetSettings GetAssetSettings(this IDictionary<string, object> environment)
        {
            return environment.Get(AssetsConstants.AssetSettings, new AssetSettings());
        }
    }
}