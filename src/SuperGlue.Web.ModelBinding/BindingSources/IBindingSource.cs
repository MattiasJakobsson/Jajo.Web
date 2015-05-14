using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public interface IBindingSource
    {
        Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment);
    }
}