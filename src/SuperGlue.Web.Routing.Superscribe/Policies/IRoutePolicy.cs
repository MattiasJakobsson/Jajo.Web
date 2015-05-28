using System.Collections.Generic;

namespace SuperGlue.Web.Routing.Superscribe.Policies
{
    public interface IRoutePolicy
    {
        void BuildRoutes(IDictionary<string, object> environment);
    }
}