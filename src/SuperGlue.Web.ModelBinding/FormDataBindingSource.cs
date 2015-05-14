using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public class FormDataBindingSource : IBindingSource
    {
        public async Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            var form = await envinronment.GetRequest().ReadForm();
            return form.ToDictionary(x => x.Key.ToLower(), x => (object)form.Get(x.Key));
        }
    }
}