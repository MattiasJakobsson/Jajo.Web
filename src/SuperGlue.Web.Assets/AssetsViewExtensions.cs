using System.IO;
using HtmlTags;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public static class AssetsViewExtensions
    {
        public static void Asset(this ISuperGlueView view, string assetName)
        {
            view.Environment.Resolve<IAssetRequirements>().Require(assetName);
        }

        public static ImageTag Image(this ISuperGlueView view, string imageFilename)
        {
            return new ImageTag(imageFilename);
        }

        public static HtmlTag ImageFor(this ISuperGlueView view, string assetName)
        {
            //HACK:Hard coded path to assets
            var imagePath = Path.Combine("/_assets/", assetName);

            return new HtmlTag("img").Attr("src", imagePath);
        }

        public static TagList WriteCssTags(this ISuperGlueView view)
        {
            return view.Environment.Resolve<IAssetTagWriter>().WriteTags(MimeType.Css);
        }

        public static TagList WriteScriptTags(this ISuperGlueView view)
        {
            return view.Environment.Resolve<IAssetTagWriter>().WriteTags(MimeType.Javascript);
        }

        public static TagList WriteAssetTags(this ISuperGlueView view)
        {
            return view.Environment.Resolve<IAssetTagWriter>().WriteAllTags();
        }
    }
}