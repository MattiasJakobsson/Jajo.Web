using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class ValueConverterCollection : IValueConverterCollection
    {
        private readonly IEnumerable<IValueConverter> _valueConverters;

        public ValueConverterCollection(IEnumerable<IValueConverter> valueConverters)
        {
            _valueConverters = valueConverters;
        }

        public IEnumerator<IValueConverter> GetEnumerator()
        {
            return _valueConverters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool CanConvert(Type destinationType, IBindingContext context)
        {
            return GetMatchingConverters(destinationType).Any();
        }

        public BindingResult Convert(Type destinationType, object value, IBindingContext context)
        {
            var converter = GetMatchingConverters(destinationType).FirstOrDefault();

            if (converter == null)
            {
                context.Environment.Log("Failed to find a matching converter for type: {0}", LogLevel.Info, destinationType != null ? destinationType.Name : "null");
                return new BindingResult(null, false);
            }

            var result = converter.Convert(destinationType, value);

            context.Environment.Log("Converted value: \"{0}\" to type: {1} using converted: {2} with result: Success = {3}.", (value ?? "").ToString(), destinationType != null ? destinationType.Name : "null", converter.GetType().Name, result.Success);

            return result;
        }

        private IEnumerable<IValueConverter> GetMatchingConverters(Type destinationType)
        {
            return _valueConverters.Where(x => x.Matches(destinationType));
        } 
    }
}