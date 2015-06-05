using System.Collections.Generic;

namespace SuperGlue.Web.Routing
{
    public interface ICheckIfRouteExists
    {
        bool Exists(object routeEndpoint, IDictionary<string, object> environment);
    }
}