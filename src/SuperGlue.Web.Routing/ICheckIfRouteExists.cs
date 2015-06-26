using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Routing
{
    public interface ICheckIfRouteExists
    {
        Task<bool> Exists(object routeEndpoint, IDictionary<string, object> environment);
    }
}