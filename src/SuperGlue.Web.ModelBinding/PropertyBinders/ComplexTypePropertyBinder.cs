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

        public bool Matches(PropertyInfo propertyInfo)
        {
            return !_valueConverterCollection.CanConvert(propertyInfo.PropertyType);
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            using (bindingContext.OpenChildContext(string.Format("{0}_", propertyInfo.Name)))
            {
                var obj = await bindingContext.Bind(propertyInfo.PropertyType);
                if (obj == null)
                    return false;

                propertyInfo.SetValue(instance, obj, new object[0]);

                return true;
            }
        }
    }
}