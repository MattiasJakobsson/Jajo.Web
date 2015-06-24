using HtmlTags;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public interface IAssetTagWriter
    {
        TagList WriteAllTags();

        TagList WriteTags(MimeType mimeType, params string[] tags);
    }
}