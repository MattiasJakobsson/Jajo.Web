using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class BoolValueConverter : ParseValueConverter<bool>
    {
        protected override bool Parse(object value)
        {
            var stringValue = value.ToString();

            if (stringValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                return true;

            bool parsed;
            bool.TryParse(stringValue, out parsed);
            return parsed;
        }
    }
}