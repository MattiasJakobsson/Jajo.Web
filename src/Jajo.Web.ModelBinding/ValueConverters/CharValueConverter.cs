namespace Jajo.Web.ModelBinding.ValueConverters
{
    public class CharValueConverter : ParseValueConverter<char>
    {
        protected override char Parse(object value)
        {
            char parsed;
            char.TryParse(value.ToString(), out parsed);
            return parsed;
        }
    }
}