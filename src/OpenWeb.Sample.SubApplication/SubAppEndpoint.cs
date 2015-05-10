namespace OpenWeb.Sample.SubApplication
{
    public class SubAppEndpoint
    {
        public SubAppEndpointQueryResult Query()
        {
            return new SubAppEndpointQueryResult("Hello from subapp");
        }
    }

    public class SubAppEndpointQueryResult
    {
        public SubAppEndpointQueryResult(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
