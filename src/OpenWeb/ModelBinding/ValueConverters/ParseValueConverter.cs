using System;

namespace OpenWeb.ModelBinding.ValueConverters
{
    public abstract class ParseValueConverter<T> : IValueConverter where T : struct
    {
        public virtual bool Matches(Type destinationType)
        {
            return destinationType == typeof (T);
        }

        public virtual object Convert(Type destinationType, object value)
        {
            if (value == null) return null;

            if (value is T)
                return (T) value;

            return Parse(value);
        }

        protected abstract T Parse(object value);
    }
}