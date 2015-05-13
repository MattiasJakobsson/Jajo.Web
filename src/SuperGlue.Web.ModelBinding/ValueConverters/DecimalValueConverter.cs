namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class DecimalValueConverter : ParseValueConverter<decimal>
    {
        protected override decimal Parse(object value)
        {
            decimal parsed;
            decimal.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}