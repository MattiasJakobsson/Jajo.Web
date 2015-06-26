using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.PartialRequests
{
    public class MakeSureWeDontExecutePartialsFromRegularRequest : ICheckIfRouteExists
    {
        public Task<bool> Exists(object routeEndpoint, IDictionary<string, object> environment)
        {
            return Task.Factory.StartNew(() =>
            {
                if (environment.IsPartial())
                    return true;

                return !environment.IsPartialEndpoint(routeEndpoint);
            });
        }
    }
}