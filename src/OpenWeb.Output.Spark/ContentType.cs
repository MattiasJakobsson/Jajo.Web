using System.Net.Mime;

namespace OpenWeb.Output.Spark
{
    public class ContentType
    {
        public static readonly ContentType Html = new ContentType(MediaTypeNames.Text.Html);
        public static readonly ContentType Json = new ContentType("application/json");
        public static readonly ContentType Text = new ContentType(MediaTypeNames.Text.Plain);
        public static readonly ContentType Javascript = new ContentType("text/javascript");

        private readonly string _mimeType;

        public ContentType(string mimeType)
        {
            _mimeType = mimeType;
        }

        public override string ToString()
        {
            return _mimeType;
        }
    }
}