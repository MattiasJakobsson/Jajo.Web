namespace SuperGlue.RavenDb.Search
{
    public interface IParseSearchString
    {
        SearchParsingResult Parse(string search, params string[] specialCommands);
    }
}