using System.Collections.Generic;

namespace SuperGlue.Web.Routing.Superscribe.Conventional
{
    public interface ICheckIfRouteExists
    {
        bool Exists(object routeEndpoint, IDictionary<string, object> environment);
    }
}