using System.Collections.Generic;

namespace SuperGlue.Web.Routing
{
    public interface IRoutePolicy
    {
        IEnumerable<EndpointInformation> Build();
    }
}