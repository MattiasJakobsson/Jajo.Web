using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarkdownSharp;

namespace SuperGlue.ContentParsing.Markdown
{
    public class MarkdowTextParser : RegexTextParser
    {
        protected override Task<object> FindParameterValue(IDictionary<string, object> environment, Match match, Func<string, string> recurse)
        {
            var markdownParser = new MarkdownSharp.Markdown(new MarkdownOptions
            {
                AutoNewLines = true
            });

            var mardown = match.Groups[1].Value;

            return Task.FromResult<object>(string.IsNullOrEmpty(mardown) ? mardown : markdownParser.Transform(mardown));
        }

        protected override IEnumerable<Regex> GetRegexes()
        {
            yield return new Regex(@"\<md\>(.*?)\<\/md\>", RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}