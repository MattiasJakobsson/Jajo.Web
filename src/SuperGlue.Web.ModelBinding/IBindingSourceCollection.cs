using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingSourceCollection : IEnumerable<IBindingSource>
    {
        Task<bool> ContainsKey(string key, IDictionary<string, object> environment);
        Task<object> Get(string key, IDictionary<string, object> environment);
    }
}