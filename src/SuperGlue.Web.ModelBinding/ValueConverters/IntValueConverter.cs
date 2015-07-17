namespace SuperGlue.Web.ModelBinding.ValueConverters
{
    public class IntValueConverter : ParseValueConverter<int>
    {
        protected override int Parse(object value)
        {
            int parsed;
            int.TryParse((value ?? "").ToString(), out parsed);
            return parsed;
        }
    }
}