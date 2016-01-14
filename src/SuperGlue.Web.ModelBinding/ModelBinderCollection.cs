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
            if(_valueConverterCollection.CanConvert(type, bindingContext))
                return _valueConverterCollection.Convert(type, await _bindingSourceCollection.Get(bindingContext.GetPrefix(), bindingContext.Environment).ConfigureAwait(false), bindingContext);

            var binder = GetMatchingBinders(type).FirstOrDefault();

            if (binder == null)
            {
                bindingContext.Environment.Log("Failed to find a matching modelbinder for type: {0}", LogLevel.Info, type);
                return new BindingResult(null, false);
            }

            bindingContext.Environment.Log("Going to bind type: {0} using {1}.", LogLevel.Debug, type, binder.GetType().Name);

            var result = await binder.Bind(type, bindingContext).ConfigureAwait(false);

            bindingContext.Environment.Log("Finished binding type: {0} using {1}. Result: Success = {2}, Instance = {3}.", LogLevel.Debug, type, binder.GetType().Name, result.Success, result.Instance?.ToString() ?? "null");

            return result;
        }

        private IEnumerable<IModelBinder> GetMatchingBinders(Type type)
        {
            return _modelBinders.Where(x => x.Matches(type));
        }
    }
}