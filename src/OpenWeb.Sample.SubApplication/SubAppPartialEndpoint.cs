namespace OpenWeb.Sample.SubApplication
{
    public class SubAppPartialEndpoint
    {
        public SubAppPartialEndpointQueryResult Query(SubAppPartialEndpointQueryInput input)
        {

            return new SubAppPartialEndpointQueryResult("Test partial in subapp");
        }
    }

    public class SubAppPartialEndpointQueryInput
    {
        public string Slug { get; set; }
        public int Id { get; set; }
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