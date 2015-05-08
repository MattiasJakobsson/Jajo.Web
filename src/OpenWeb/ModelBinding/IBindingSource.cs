using System.Collections.Generic;

namespace OpenWeb.ModelBinding
{
    public interface IBindingSource
    {
        IDictionary<string, object> GetValues();
        IEnumerable<string> GetKeys();
    }
}