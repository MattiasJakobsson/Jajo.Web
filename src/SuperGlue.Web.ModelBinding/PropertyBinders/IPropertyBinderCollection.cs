using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.PropertyBinders
{
    public interface IPropertyBinderCollection : IEnumerable<IPropertyBinder>
    {
        Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext);
    }
}