using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SuperGlue.RavenDb.Search
{
    public class DefaultSearchStringParser : IParseSearchString
    {
        private static readonly Regex SentencePattern = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

        public SearchParsingResult Parse(string search, params string[] specialCommands)
        {
            search = search ?? "";

            var partSeperator = GetPartSeperator();
            var parts = new List<SearchPart>();
            var freeSearches = new List<string>();
            var specials = new List<string>();

            search = SentencePattern
                .Replace(search, x => x.Value.Replace(' ', '|').Replace(partSeperator, '|'))
                .Replace("\"", "");

            var searchParts = search.Split(' ');

            foreach (var part in searchParts)
            {
                var currentPart = part.Replace('|', ' ');

                if (specialCommands.Any(x => x == currentPart))
                {
                    specials.Add(currentPart);
                    continue;
                }

                var partParts = currentPart.Split(partSeperator);

                if (partParts.Length == 2)
                {
                    parts.Add(new SearchPart(partParts[0], partParts[1]));
                    continue;
                }

                freeSearches.Add(currentPart);
            }

            return new SearchParsingResult(search, freeSearches, parts, specials);
        }

        protected virtual char GetPartSeperator()
        {
            return ':';
        }
    }
}