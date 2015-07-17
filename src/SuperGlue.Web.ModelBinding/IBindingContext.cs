using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingContext
    {
        Task<BindingResult> Bind(Type type);
        void PrefixWith(string prefix);
        string GetKey(string name);
        string GetPrefix();
        IDisposable OpenChildContext(string prefix);

        IDictionary<string, object> Environment { get; }
    }
}