namespace Jajo.Web.ModelBinding.ValueConverters
{
    public class UIntValueConverter : ParseValueConverter<uint>
    {
        protected override uint Parse(object value)
        {
            uint parsed;
            uint.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}