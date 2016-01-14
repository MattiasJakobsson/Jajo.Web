using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding.PropertyBinders
{
    public class PropertyBinderCollection : IPropertyBinderCollection
    {
        private readonly IEnumerable<IPropertyBinder> _propertyBinders;

        public PropertyBinderCollection(IEnumerable<IPropertyBinder> propertyBinders)
        {
            _propertyBinders = propertyBinders;
        }

        public IEnumerator<IPropertyBinder> GetEnumerator()
        {
            return _propertyBinders.GetEnumerator();
        }

        public async Task<bool> Bind(object instance, PropertyInfo propertyInfo, IBindingContext bindingContext)
        {
            var binder = _propertyBinders.FirstOrDefault(x => x.Matches(propertyInfo, bindingContext));

            if (binder == null)
            {
                bindingContext.Environment.Log("Failed to find a propertybinder for property: {0} on: {1}.", LogLevel.Info, propertyInfo.Name, propertyInfo.DeclaringType?.Name ?? "");
                return false;
            }

            bindingContext.Environment.Log("Going to bind property: {0} on: {1} using: {2}.", LogLevel.Debug, propertyInfo.Name, propertyInfo.DeclaringType?.Name ?? "", binder);

            var result = await binder.Bind(instance, propertyInfo, bindingContext).ConfigureAwait(false);

            bindingContext.Environment.Log("Finished binding property: {0} on: {1} using: {2} with result: Success = {3}.", LogLevel.Debug, propertyInfo.Name, propertyInfo.DeclaringType?.Name ?? "", binder, result);

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}