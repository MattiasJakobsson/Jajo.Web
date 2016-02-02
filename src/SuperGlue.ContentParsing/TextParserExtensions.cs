using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.ContentParsing
{
    public static class TextParserExtensions
    {
        public static async Task<string> ParseText(this IEnumerable<ITextParser> parsers, IDictionary<string, object> environment, string text)
        {
            var parserList = parsers.ToList();

            foreach (var parser in parserList)
                text = await parser.Parse(environment, text, x => ParseText(parserList, environment, x)).ConfigureAwait(false);

            return text;
        }
    }
}