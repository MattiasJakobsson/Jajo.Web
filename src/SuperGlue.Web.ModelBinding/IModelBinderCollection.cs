using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IModelBinderCollection : IEnumerable<IModelBinder>
    {
        Task<object> Bind(Type type, IBindingContext bindingContext);
    }
}