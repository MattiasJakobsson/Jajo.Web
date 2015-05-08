namespace OpenWeb.ModelBinding.ValueConverters
{
    public class UShortValueConverter : ParseValueConverter<ushort>
    {
        protected override ushort Parse(object value)
        {
            ushort parsed;
            ushort.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}