using System.IO;
using System.Threading.Tasks;

namespace SuperGlue.Web.FileUpload
{
    public interface ITransformFile
    {
        Task<Stream> Transform(Stream content);
        string GetFileName(string originalName);
    }
}