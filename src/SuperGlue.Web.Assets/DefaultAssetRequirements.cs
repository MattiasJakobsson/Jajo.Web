using System.Collections.Generic;
using System.Linq;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public class DefaultAssetRequirements : IAssetRequirements
    {
        private readonly List<string> _rendered = new List<string>();
        private readonly List<string> _requirements = new List<string>();

        public IEnumerable<string> AllRequestedAssets => _requirements;

        public IEnumerable<string> AllRenderedAssets => _rendered;

        public void Require(params string[] name)
        {
            name.Each(x =>
            {
                if(!_requirements.Contains(x))
                    _requirements.Add(x);
            });
        }

        public AssetPlanKey DequeueAssetsToRender(MimeType mimeType)
        {
            var requested = OutstandingAssets().Where(x => MimeType.MimeTypeByFileName(x) == mimeType);
            var names = ReturnOrderedDependenciesFor(mimeType, requested);
            return new AssetPlanKey(mimeType, names);
        }

        public IEnumerable<AssetPlanKey> DequeueAssetsToRender()
        {
            return OutstandingAssets().Select(MimeType.MimeTypeByFileName).Distinct().ToList().Select(DequeueAssetsToRender).ToList();
        }

        public AssetPlanKey DequeueAssets(MimeType mimeType, params string[] assets)
        {
            var list = assets.Except(_rendered).ToList();
            _rendered.Fill(list);
            return new AssetPlanKey(mimeType, list);
        }

        private IEnumerable<string> OutstandingAssets()
        {
            return _requirements.Except(_rendered).ToList();
        }

        private IEnumerable<string> ReturnOrderedDependenciesFor(MimeType mimeType, IEnumerable<string> requested)
        {
            var list = requested.ToList();
            
            if (mimeType != null)
                list.RemoveAll(x => MimeType.MimeTypeByFileName(x) != mimeType);
            
            list.RemoveAll(x => _rendered.Contains(x));
            _rendered.Fill(list);
            return list;
        }
    }
}