using System.Collections.Generic;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.PartialRequests
{
    public class MakeSureWeDontExecutePartialsFromRegularRequest : ICheckIfRouteExists
    {
        public bool Exists(object routeEndpoint, IDictionary<string, object> environment)
        {
            if (environment.IsPartial())
                return true;

            return !environment.IsPartialEndpoint(routeEndpoint);
        }
    }
}