using System;
using System.Collections.Generic;

namespace SuperGlue.Web.ModelBinding
{
    public interface IBindingContext
    {
        object Bind(Type type);
        void Bind(Type type, object instance);
        void PrefixWith(string prefix);
        string GetKey(string name);
        string GetPrefix();
        IDisposable OpenChildContext(string prefix);

        IDictionary<string, object> Environment { get; }
    }
}