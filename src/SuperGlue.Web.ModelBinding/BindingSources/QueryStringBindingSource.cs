using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public class QueryStringBindingSource : IBindingSource
    {
        public Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.FromResult((IDictionary<string, object>)envinronment.GetRequest().Query.ToDictionary(x => x.Key.ToLower(), x => (object)x.Value));
        }
    }
}