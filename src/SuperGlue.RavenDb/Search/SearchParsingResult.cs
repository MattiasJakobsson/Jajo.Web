using System.Collections.Generic;

namespace SuperGlue.RavenDb.Search
{
    public class SearchParsingResult
    {
        public SearchParsingResult(string original, IEnumerable<string> freeSearch, IEnumerable<SearchPart> parts, IEnumerable<string> specialCommands)
        {
            SpecialCommands = specialCommands;
            Parts = parts;
            FreeSearch = freeSearch;
            Original = original;
        }

        public string Original { get; private set; }
        public IEnumerable<string> FreeSearch { get; private set; }
        public IEnumerable<SearchPart> Parts { get; private set; }
        public IEnumerable<string> SpecialCommands { get; private set; }
    }
}