using System;
using System.Collections.Generic;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public interface IAssetRequirements
    {
        IEnumerable<string> AllRequestedAssets { get; }

        IEnumerable<string> AllRenderedAssets { get; }

        void Require(params string[] name);

        AssetPlanKey DequeueAssetsToRender(MimeType mimeType);

        IEnumerable<AssetPlanKey> DequeueAssetsToRender();

        AssetPlanKey DequeueAssets(MimeType mimeType, params string[] assets);
    }
}