namespace OpenWeb.ModelBinding.ValueConverters
{
    public class FloatValueConverter : ParseValueConverter<float>
    {
        protected override float Parse(object value)
        {
            float parsed;
            float.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}