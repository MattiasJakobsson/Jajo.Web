using HtmlTags;
using SuperGlue.Configuration;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.FileUpload
{
    public static class FileUploadViewExtensions
    {
        public static string FileUrl(this ISuperGlueView view, string fileName, ITransformFile transformer = null)
        {
            return view.Environment.Resolve<IPersistFiles>().GetUrlFor(view.Environment, fileName, transformer);
        }

        public static HtmlTag UploadedImage(this ISuperGlueView view, string fileName, ITransformFile transformer = null)
        {
            return new HtmlTag("img")
                .Attr("src", FileUrl(view, fileName, transformer));
        }
    }
}