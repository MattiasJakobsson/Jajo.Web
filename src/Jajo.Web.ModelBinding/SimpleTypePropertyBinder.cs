using System.Reflection;

namespace Jajo.Web.ModelBinding
{
    public class SimpleTypePropertyBinder : IPropertyBinder
    {
        private readonly IValueConverterCollection _valueConverterCollection;
        private readonly IBindingSourceCollection _bindingSourceCollection;

        public SimpleTypePropertyBinder(IValueConverterCollection valueConverterCollection, IBindingSourceCollection bindingSourceCollection)
        {
            _valueConverterCollection = valueConverterCollection;
            _bindingSourceCollection = bindingSourceCollection;
        }

        public bool Matches(PropertyInfo propertyInfo)
        {
            return _valueConverterCollection.CanConvert(propertyInfo.PropertyType);
        }

        public bool Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            if (!_bindingSourceCollection.ContainsKey(bindingContext.GetKey(propertyInfo.Name)))
                return false;

            propertyInfo.SetValue(instance, _valueConverterCollection.Convert(propertyInfo.PropertyType,
                                                                             _bindingSourceCollection.Get(bindingContext.GetKey(propertyInfo.Name))), new object[0]);

            return true;
        }
    }
}