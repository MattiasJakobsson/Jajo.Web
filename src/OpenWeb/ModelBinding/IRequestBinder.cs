using System;

namespace OpenWeb.ModelBinding
{
    public interface IRequestBinder
    {
        T Get<T>();
        object Get(Type type);
        void Set<T>(T input);
    }
}