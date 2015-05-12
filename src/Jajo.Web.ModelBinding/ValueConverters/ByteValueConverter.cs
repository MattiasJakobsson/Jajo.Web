namespace Jajo.Web.ModelBinding.ValueConverters
{
    public class ByteValueConverter : ParseValueConverter<byte>
    {
        protected override byte Parse(object value)
        {
            byte parsed;
            byte.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}