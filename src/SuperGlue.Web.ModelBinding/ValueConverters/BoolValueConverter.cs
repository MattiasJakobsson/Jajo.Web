using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class BoolValueConverter : ParseValueConverter<bool>
    {
        protected override bool Parse(string stringValue, out bool success)
        {
            if (stringValue.Equals("on", StringComparison.InvariantCultureIgnoreCase))
            {
                success = true;
                return true;
            }

            bool parsed;
            success = bool.TryParse(stringValue, out parsed);

            return parsed;
        }
    }
}