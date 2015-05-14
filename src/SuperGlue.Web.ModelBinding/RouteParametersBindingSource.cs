using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public class RouteParametersBindingSource : IBindingSource
    {
        public Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.Factory.StartNew(() => (IDictionary<string, object>)envinronment.GetRouteInformation().Parameters.ToDictionary(x => x.Key, x => x.Value));
        }
    }
}