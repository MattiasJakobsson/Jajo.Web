namespace OpenWeb.Sample.SubApplication
{
    public class SubAppPartialEndpoint
    {
        public SubAppPartialEndpointQueryResult Query()
        {
            return new SubAppPartialEndpointQueryResult("Test partial in subapp");
        }
    }

    public class SubAppPartialEndpointQueryResult
    {
        public SubAppPartialEndpointQueryResult(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}