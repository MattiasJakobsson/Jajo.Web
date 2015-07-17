using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class StringValueConverter : IValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public BindingResult Convert(Type destinationType, object value)
        {
            return value == null ? new BindingResult(null, false) : new BindingResult(value.ToString(), true);
        }
    }
}