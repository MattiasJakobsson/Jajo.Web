using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class DateTimeValueConverter : ParseValueConverter<DateTime>
    {
        protected override DateTime Parse(object value)
        {
            DateTime parsed;
            DateTime.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}