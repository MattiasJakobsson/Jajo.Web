using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public class ComplexTypePropertyBinder : IPropertyBinder
    {
        public bool Matches(PropertyInfo propertyInfo)
        {
            return true;
        }

        public Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            using (bindingContext.OpenChildContext(string.Format("{0}_", propertyInfo.Name)))
            {
                var obj = bindingContext.Bind(propertyInfo.PropertyType);
                if (obj == null) 
                    return Task.Factory.StartNew(() => false);

                propertyInfo.SetValue(instance, obj, new object[0]);

                return Task.Factory.StartNew(() => true);
            }
        }
    }
}