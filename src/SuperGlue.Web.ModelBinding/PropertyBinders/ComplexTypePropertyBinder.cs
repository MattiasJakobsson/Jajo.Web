using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Web.ModelBinding.ValueConverters;

namespace SuperGlue.Web.ModelBinding.PropertyBinders
{
    public class ComplexTypePropertyBinder : IPropertyBinder
    {
        private readonly IValueConverterCollection _valueConverterCollection;

        public ComplexTypePropertyBinder(IValueConverterCollection valueConverterCollection)
        {
            _valueConverterCollection = valueConverterCollection;
        }

        public bool Matches(PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            return !_valueConverterCollection.CanConvert(propertyInfo.PropertyType, bindingContext);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            using (bindingContext.OpenChildContext(string.Format("{0}_", propertyInfo.Name)))
            {
                var result = await bindingContext.Bind(propertyInfo.PropertyType);

                if (!result.Success)
                    return false;

                propertyInfo.SetValue(instance, result.Instance, new object[0]);

                return true;
            }
        }
    }
}