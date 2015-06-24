using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlTags;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public class DefaultAssetTagWriter : IAssetTagWriter
    {
        private readonly IAssetRequirements _requirements;
        private readonly Cache<MimeType, Func<string, HtmlTag>> _builders = new Cache<MimeType, Func<string, HtmlTag>>();

        public DefaultAssetTagWriter(IAssetRequirements requirements)
        {
            _requirements = requirements;
            _builders[MimeType.Javascript] = url => new HtmlTag("script").Attr("type", "text/javascript").Attr("src", url);
            _builders[MimeType.Css] = url => new HtmlTag("link").Attr("href", url).Attr("rel", "stylesheet").Attr("type", MimeType.Css.Value);
        }

        public TagList WriteAllTags()
        {
            return _requirements.DequeueAssetsToRender().SelectMany(TagsForPlan).ToTagList();
        }

        private TagList WriteTags(MimeType mimeType)
        {
            return TagsForPlan(_requirements.DequeueAssetsToRender(mimeType)).ToTagList();
        }

        public TagList WriteTags(MimeType mimeType, params string[] tags)
        {
            return tags.Length == 0 ? WriteTags(mimeType) : TagsForPlan(_requirements.DequeueAssets(mimeType, tags)).ToTagList();
        }

        private IEnumerable<HtmlTag> TagsForPlan(AssetPlanKey key)
        {
            //HACK:Hard coded path to assets
            return key.Names.Select(asset => _builders[key.MimeType](Path.Combine("/_assets/", asset)));
        }
    }
}