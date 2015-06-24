using System.Reflection.Emit;
using HtmlTags;

namespace SuperGlue.Web.Assets
{
    public class ImageTag : HtmlTag
    {
        public ImageTag(string url) : base("img")
        {
            Attr("src", url);
        }

        public ImageTag AlternateText(StringToken token)
        {
            Attr("alt", ((object) token).ToString());
            return this;
        }

        public ImageTag Width(int width)
        {
            Style("width", (string) (object) width + (object) "px");
            return this;
        }

        public ImageTag Height(int height)
        {
            Style("height", (string) (object) height + (object) "px");
            return this;
        }
    }
}