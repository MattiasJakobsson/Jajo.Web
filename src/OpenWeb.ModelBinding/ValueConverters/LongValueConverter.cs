namespace OpenWeb.ModelBinding.ValueConverters
{
    public class LongValueConverter : ParseValueConverter<long>
    {
        protected override long Parse(object value)
        {
            long parsed;
            long.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}