namespace SuperGlue.Localization
{
    public interface ILocalizationVisitor
    {
        string AfterLocalized(string key, string value);
    }
}