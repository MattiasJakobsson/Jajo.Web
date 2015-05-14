using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingSourceCollection : IEnumerable<IBindingSource>
    {
        bool ContainsKey(string key, IDictionary<string, object> environment);
        object Get(string key, IDictionary<string, object> environment);
    }
}