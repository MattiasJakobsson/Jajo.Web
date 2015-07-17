using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class UriValueConverter : IValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (Uri);
        }

        public object Convert(Type destinationType, object value)
        {
            return new Uri((value ?? "").ToString(), UriKind.RelativeOrAbsolute);
        }
    }
}