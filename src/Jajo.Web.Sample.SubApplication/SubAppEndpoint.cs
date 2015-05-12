using System.Collections.Generic;
using System.IO;
using Jajo.Web.PartialRequests;

namespace Jajo.Web.Sample.SubApplication
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
            Input = new SubAppPartialEndpointQueryInput
            {
                Slug = "test",
                Id = 2
            };
        }

        public string Message { get; private set; }
        public SubAppPartialEndpointQueryInput Input { get; private set; }

        public string Partial(IDictionary<string, object> environment)
        {
            using (var streamReader = new StreamReader(Partials.ExecutePartial(environment, typeof(SubAppPartialEndpoint).GetMethod("Query")).Result))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
