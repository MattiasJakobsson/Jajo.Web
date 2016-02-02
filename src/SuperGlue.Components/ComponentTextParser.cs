using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperGlue.ContentParsing;

namespace SuperGlue.Components
{
    public class ComponentTextParser : RegexTextParser
    {
        private readonly IEnumerable<IComponentSource> _componentSources;

        public ComponentTextParser(IEnumerable<IComponentSource> componentSources)
        {
            _componentSources = componentSources;
        }

        protected override async Task<object> FindParameterValue(IDictionary<string, object> environment, Match match, Func<string, string> recurse)
        {
            var componentNameGroup = match.Groups["componentName"];

            if (componentNameGroup == null)
                return "";

            var componentName = componentNameGroup.Value;

            var component = await _componentSources.FindComponent(componentName, environment).ConfigureAwait(false);

            if (component == null)
                return "";

            var settingsGroup = match.Groups["settings"];

            dynamic data = null;

            if (!string.IsNullOrEmpty(settingsGroup?.Value))
            {
                var settingsJson = settingsGroup.Value;

                if (!string.IsNullOrEmpty(settingsJson))
                    data = JsonConvert.DeserializeObject<dynamic>(settingsJson);
            }

            var result = await Read(environment, data, component).ConfigureAwait(false);

            return recurse(result);
        }

        protected override IEnumerable<Regex> GetRegexes()
        {
            yield return new Regex(@"\!\[Components\.((?<componentName>.[a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9.""\]\[]*))\]\!", RegexOptions.Compiled);
            yield return new Regex(@"\!\[Components\.((?<componentName>.[a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9.""\]\[]*)) Settings\=((?<settings>[a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9.""\]\[\{\}]*))\]\!", RegexOptions.Compiled);
        }

        private static async Task<string> Read(IDictionary<string, object> environment, dynamic data, IComponent component)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            await component.RenderTo(environment, data, writer).ConfigureAwait(false);

            await writer.FlushAsync().ConfigureAwait(false);
            stream.Position = 0;

            using (var reader = new StreamReader(stream))
                return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}