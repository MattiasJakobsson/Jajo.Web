using System;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Web.ModelBinding.PropertyBinders;

namespace SuperGlue.Web.ModelBinding
{
    public class DefaultModelBinder : IModelBinder
    {
        private readonly IPropertyBinderCollection _propertyBinderCollection;

        public DefaultModelBinder(IPropertyBinderCollection propertyBinderCollection)
        {
            _propertyBinderCollection = propertyBinderCollection;
        }

        public bool Matches(Type type)
        {
            return true;
        }

        public async Task<object> Bind(Type type, IBindingContext bindingContext)
        {
            var instance = Activator.CreateInstance(type);
            return await Bind(type, bindingContext, instance) ? instance : null;
        }

        public async Task<bool> Bind(Type type, IBindingContext bindingContext, object instance)
        {
            var hasBeenBound = false;

            foreach (var property in type.GetProperties().Where(x => x.CanWrite).Select(x => new { PropertyInfo = x, Binders = _propertyBinderCollection.GetMatching(x) }).Where(property => property.Binders != null))
            {
                if (await property.Binders.Bind(instance, property.PropertyInfo, bindingContext))
                    hasBeenBound = true;
            }

            return hasBeenBound;
        }
    }
}