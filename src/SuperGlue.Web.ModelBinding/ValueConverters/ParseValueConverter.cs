using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public abstract class ParseValueConverter<T> : IValueConverter where T : struct
    {
        public virtual bool Matches(Type destinationType)
        {
            return destinationType == typeof (T);
        }

        public virtual BindingResult Convert(Type destinationType, object value)
        {
            if (value == null) 
                return new BindingResult(null, false);

            if (value is T)
                return new BindingResult((T)value, true);

            bool success;
            var result = Parse(value.ToString(), out success);

            return new BindingResult(success ? result : default(T), success);
        }

        protected abstract T Parse(string stringValue, out bool success);
    }
}