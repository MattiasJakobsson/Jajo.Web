using System;

namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class TimeSpanValueConverter : ParseValueConverter<TimeSpan>
    {
        protected override TimeSpan Parse(object value)
        {
            TimeSpan timeSpan;
            TimeSpan.TryParse((value ?? "").ToString(), out timeSpan);

            return timeSpan;
        }
    }
}