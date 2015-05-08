using System;
using System.Collections.Generic;

namespace OpenWeb.ModelBinding
{
    public interface IModelBinderCollection : IEnumerable<IModelBinder>
    {
        object Bind(Type type, IBindingContext bindingContext);
        bool Bind(Type type, IBindingContext bindingContext, object instance);
    }
}