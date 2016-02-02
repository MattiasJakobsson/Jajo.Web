using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperGlue.ContentParsing
{
    public abstract class RegexTextParser : ITextParser
    {
        protected virtual string SeperateListItemsWith => "\n";

        public async Task<string> Parse(IDictionary<string, object> environment, string text, Func<string, string> recurse)
        {
            text = text ?? "";

            foreach (var regex in GetRegexes())
            {
                text = await regex.ReplaceAsync(text, async x =>
                {
                    var value = await FindParameterValue(environment, x, recurse).ConfigureAwait(false);

                    if (value == null) return "";

                    var enumerableValue = value as IEnumerable;
                    if (enumerableValue == null || value is string)
                        return value.ToString();

                    var stringValues = enumerableValue.OfType<object>().Select(y => y.ToString()).ToList();

                    return string.Join(SeperateListItemsWith, stringValues);
                }).ConfigureAwait(false);
            }

            return text;
        }
        
        protected abstract Task<object> FindParameterValue(IDictionary<string, object> environment, Match match, Func<string, string> recurse);
        protected abstract IEnumerable<Regex> GetRegexes();
    }
}