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
            return type.GetConstructors().Count(x => x.GetParameters().Length == 0) == 1;
        }

        public async Task<BindingResult> Bind(Type type, IBindingContext bindingContext)
        {
            var instance = Activator.CreateInstance(type);

            var binderTasks = type
                .GetProperties()
                .Where(x => x.CanWrite)
                .Select(x => new {PropertyInfo = x, Binders = _propertyBinderCollection.GetMatching(x)})
                .Where(property => property.Binders != null)
                .Select(property => property.Binders.Bind(instance, property.PropertyInfo, bindingContext))
                .ToList();

            var success = (await Task.WhenAll(binderTasks)).Any(x => x);

            return new BindingResult(instance, success);
        }
    }
}