using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OpenWeb.PartialRequests;

namespace OpenWeb.Sample.SubApplication
{
    public class SubAppEndpoint
    {
        public SubAppEndpointQueryResult Query()
        {
            return new SubAppEndpointQueryResult("Hello from subapp", typeof(SubAppPartialEndpoint).GetMethod("Query"));
        }
    }

    public class SubAppEndpointQueryResult
    {
        public SubAppEndpointQueryResult(string message, MethodInfo routeTo)
        {
            Message = message;
            RouteTo = routeTo;
        }

        public string Message { get; private set; }
        public MethodInfo RouteTo { get; private set; }

        public string Partial(IDictionary<string, object> environment)
        {
            using (var streamReader = new StreamReader(Partials.ExecutePartial(environment, typeof(SubAppPartialEndpoint).GetMethod("Query")).Result))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
