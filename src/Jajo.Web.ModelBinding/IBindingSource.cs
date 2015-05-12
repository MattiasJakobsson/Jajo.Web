using System.Collections.Generic;

namespace Jajo.Web.ModelBinding
{
    public interface IBindingSource
    {
        IDictionary<string, object> GetValues();
        IEnumerable<string> GetKeys();
    }
}