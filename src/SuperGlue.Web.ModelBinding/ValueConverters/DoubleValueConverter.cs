namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class DoubleValueConverter : ParseValueConverter<double>
    {
        protected override double Parse(object value)
        {
            double parsed;
            double.TryParse((value ?? "").ToString(), out parsed);
            return parsed;
        }
    }
}