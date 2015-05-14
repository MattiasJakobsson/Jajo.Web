using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingSource
    {
        IDictionary<string, object> GetValues(IDictionary<string, object> envinronment);
    }
}