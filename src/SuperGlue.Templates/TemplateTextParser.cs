using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SuperGlue.ContentParsing;

namespace SuperGlue.Templates
{
    public class TemplateTextParser : RegexTextParser
    {
        private readonly IEnumerable<ITemplateSource> _templateSources;

        public TemplateTextParser(IEnumerable<ITemplateSource> templateSources)
        {
            _templateSources = templateSources;
        }

        protected override async Task<object> FindParameterValue(IDictionary<string, object> environment, Match match, Func<string, Task<string>> recurse)
        {
            var templateNameGroup = match.Groups["templateName"];

            if (templateNameGroup == null)
                return "";

            var templateName = templateNameGroup.Value;

            var template = await _templateSources.FindTemplate(templateName, environment).ConfigureAwait(false);

            if (string.IsNullOrEmpty(template))
                return "";

            return await recurse(template).ConfigureAwait(false);
        }

        protected override IEnumerable<Regex> GetRegexes()
        {
            yield return new Regex(@"\!\[Templates\.((?<templateName>.[a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9.\-]*))\]\!", RegexOptions.Compiled);
        }
    }
}