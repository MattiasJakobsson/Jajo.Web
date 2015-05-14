using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public interface IPropertyBinder
    {
        bool Matches(PropertyInfo propertyInfo);
        Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext);
    }
}