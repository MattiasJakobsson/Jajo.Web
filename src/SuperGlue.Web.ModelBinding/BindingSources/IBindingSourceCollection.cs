using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public interface IBindingSourceCollection : IEnumerable<IBindingSource>
    {
        Task<bool> ContainsKey(string key, IDictionary<string, object> environment);
        Task<object> Get(string key, IDictionary<string, object> environment);
    }
}