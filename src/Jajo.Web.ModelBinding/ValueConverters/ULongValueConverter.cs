namespace Jajo.Web.ModelBinding.ValueConverters
{
    public class ULongValueConverter : ParseValueConverter<ulong>
    {
        protected override ulong Parse(object value)
        {
            ulong parsed;
            ulong.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}