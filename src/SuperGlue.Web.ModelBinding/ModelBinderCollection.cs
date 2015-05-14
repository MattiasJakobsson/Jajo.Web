using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.ModelBinding
{
    public class ModelBinderCollection : IModelBinderCollection
    {
        private readonly IEnumerable<IModelBinder> _modelBinders;

        public ModelBinderCollection(IEnumerable<IModelBinder> modelBinders)
        {
            _modelBinders = modelBinders;
        }

        public IEnumerator<IModelBinder> GetEnumerator()
        {
            return _modelBinders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Task<object> Bind(Type type, IBindingContext bindingContext)
        {
            var binder = GetMatchingBinders(type).FirstOrDefault();

            return binder == null ? null : binder.Bind(type, bindingContext);
        }

        public async Task<bool> Bind(Type type, IBindingContext bindingContext, object instance)
        {
            var binder = GetMatchingBinders(type).FirstOrDefault();

            return binder != null && await binder.Bind(type, bindingContext, instance);
        }

        private IEnumerable<IModelBinder> GetMatchingBinders(Type type)
        {
            return _modelBinders.Where(x => x.Matches(type));
        }
    }
}