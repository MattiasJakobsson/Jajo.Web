using System.Reflection;

namespace OpenWeb.ModelBinding
{
    public interface IPropertyBinder
    {
        bool Matches(PropertyInfo propertyInfo);
        bool Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext);
    }
}