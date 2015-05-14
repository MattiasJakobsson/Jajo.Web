using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public class CookieBindingSource : IBindingSource
    {
        public Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            return Task.Factory.StartNew(() => (IDictionary<string, object>)envinronment.GetRequest().Cookies.ToDictionary(x => x.Key.ToLower(), x => (object)x.Value));
        }
    }
}