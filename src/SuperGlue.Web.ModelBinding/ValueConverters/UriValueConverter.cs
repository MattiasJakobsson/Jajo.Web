using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class UriValueConverter : IValueConverter
    {
        public bool Matches(Type destinationType)
        {
            return destinationType == typeof (Uri);
        }

        public BindingResult Convert(Type destinationType, object value)
        {
            if(value == null)
                return new BindingResult(null, false);

            Uri result;
            var success = Uri.TryCreate(value.ToString(), UriKind.RelativeOrAbsolute, out result);

            return new BindingResult(result, success);
            
        }
    }
}