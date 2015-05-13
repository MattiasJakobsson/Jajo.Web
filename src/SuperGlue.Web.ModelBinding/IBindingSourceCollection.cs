using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingSourceCollection : IEnumerable<IBindingSource>
    {
        bool ContainsKey(string key);
        object Get(string key);
        IEnumerable<string> GetKeys();
    }
}