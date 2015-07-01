using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingContext
    {
        Task<object> Bind(Type type);
        Task Bind(Type type, object instance);
        void PrefixWith(string prefix);
        string GetKey(string name);
        string GetPrefix();
        IDisposable OpenChildContext(string prefix);

        IDictionary<string, object> Environment { get; }
    }
}