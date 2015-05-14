using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
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

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            if (!await _bindingSourceCollection.ContainsKey(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment))
                return false;

            propertyInfo.SetValue(instance, _valueConverterCollection.Convert(propertyInfo.PropertyType, _bindingSourceCollection.Get(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment)), new object[0]);

            return true;
        }
    }
}