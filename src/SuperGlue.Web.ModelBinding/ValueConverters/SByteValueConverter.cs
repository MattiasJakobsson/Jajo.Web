namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class SByteValueConverter : ParseValueConverter<sbyte>
    {
        protected override sbyte Parse(object value)
        {
            sbyte parsed;
            sbyte.TryParse((value ?? "").ToString(), out parsed);
            return parsed;
        }
    }
}