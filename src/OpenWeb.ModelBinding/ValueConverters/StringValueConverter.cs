using System;

namespace OpenWeb.ModelBinding.ValueConverters
{
    public class StringValueConverter : IValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public object Convert(Type destinationType, object value)
        {
            return value.ToString();
        }
    }
}