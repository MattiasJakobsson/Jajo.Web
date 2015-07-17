using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Web.ModelBinding.BindingSources;
using SuperGlue.Web.ModelBinding.ValueConverters;

namespace SuperGlue.Web.ModelBinding
{
    public class ModelBinderCollection : IModelBinderCollection
    {
        private readonly IEnumerable<IModelBinder> _modelBinders;
        private readonly IValueConverterCollection _valueConverterCollection;
        private readonly IBindingSourceCollection _bindingSourceCollection;

        public ModelBinderCollection(IEnumerable<IModelBinder> modelBinders, IValueConverterCollection valueConverterCollection, IBindingSourceCollection bindingSourceCollection)
        {
            _modelBinders = modelBinders;
            _valueConverterCollection = valueConverterCollection;
            _bindingSourceCollection = bindingSourceCollection;
        }

        public IEnumerator<IModelBinder> GetEnumerator()
        {
            return _modelBinders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task<BindingResult> Bind(Type type, IBindingContext bindingContext)
        {
            if(_valueConverterCollection.CanConvert(type))
                return _valueConverterCollection.Convert(type, await _bindingSourceCollection.Get(bindingContext.GetPrefix(), bindingContext.Environment));

            var binder = GetMatchingBinders(type).FirstOrDefault();

            return binder == null ? new BindingResult(null, false) : await binder.Bind(type, bindingContext);
        }

        private IEnumerable<IModelBinder> GetMatchingBinders(Type type)
        {
            return _modelBinders.Where(x => x.Matches(type));
        }
    }
}