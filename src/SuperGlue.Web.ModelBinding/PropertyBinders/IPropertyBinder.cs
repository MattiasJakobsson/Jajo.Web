using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.PropertyBinders
{
    public interface IPropertyBinder
    {
        bool Matches(PropertyInfo propertyInfo, IBindingContext context);
        Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext);
    }
}