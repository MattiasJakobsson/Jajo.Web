using System.IO;

namespace SuperGlue.Web.Output
{
    public class OutputRenderingResult
    {
        public OutputRenderingResult(Stream body, string contentType)
        {
            Body = body;
            ContentType = contentType;
        }

        public Stream Body { get; private set; }
        public string ContentType { get; private set; }
    }
}