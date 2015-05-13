using System.Reflection;

namespace SuperGlue.Web.ModelBinding
{
    public class ComplexTypePropertyBinder : IPropertyBinder
    {
        public bool Matches(PropertyInfo propertyInfo)
        {
            return true;
        }

        public bool Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            using (bindingContext.OpenChildContext(string.Format("{0}_", propertyInfo.Name)))
            {
                var obj = bindingContext.Bind(propertyInfo.PropertyType);
                if (obj == null) return false;

                propertyInfo.SetValue(instance, obj, new object[0]);
                return true;
            }
        }
    }
}