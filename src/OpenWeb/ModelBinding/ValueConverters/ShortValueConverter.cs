namespace OpenWeb.ModelBinding.ValueConverters
{
    public class ShortValueConverter : ParseValueConverter<short>
    {
        protected override short Parse(object value)
        {
            short parsed;
            short.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}