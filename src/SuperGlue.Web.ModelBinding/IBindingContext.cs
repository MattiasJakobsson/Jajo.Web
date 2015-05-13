using System;

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
    }
}