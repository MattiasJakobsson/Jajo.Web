using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Web.ModelBinding.BindingSources;
using SuperGlue.Web.ModelBinding.ValueConverters;

namespace SuperGlue.Web.ModelBinding.PropertyBinders
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

        public bool Matches(PropertyInfo propertyInfo, IBindingContext context)
        {
            return _valueConverterCollection.CanConvert(propertyInfo.PropertyType, context);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            if (!await _bindingSourceCollection.ContainsKey(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment))
                return false;

            var conversionResult = _valueConverterCollection.Convert(propertyInfo.PropertyType, await _bindingSourceCollection.Get(bindingContext.GetKey(propertyInfo.Name), bindingContext.Environment), bindingContext);

            if (!conversionResult.Success)
                return false;

            propertyInfo.SetValue(instance, conversionResult.Instance, new object[0]);

            return true;
        }
    }
}