namespace SuperGlue.Web.Output
{
    public class OutputRenderingResult
    {
        public OutputRenderingResult(string body, string contentType)
        {
            Body = body;
            ContentType = contentType;
        }

        public string Body { get; private set; }
        public string ContentType { get; private set; }
    }
}