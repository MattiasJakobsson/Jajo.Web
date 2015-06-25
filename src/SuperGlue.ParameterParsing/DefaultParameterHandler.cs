using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperGlue.ParameterParsing
{
    public class DefaultParameterHandler : IHandleParameters
    {
        private readonly IEnumerable<IFindParameterValue> _findParameterValues;
        private static readonly Regex ParameterRegEx = new Regex(@"\$\{((?<expression>.[a-z&auml;&aring;&ouml;A-Z&Auml;&Aring;&Ouml;0-9.""\]\[]*))\}", RegexOptions.Compiled);

        public DefaultParameterHandler(IEnumerable<IFindParameterValue> findParameterValues)
        {
            _findParameterValues = findParameterValues;
        }

        public Task<string> ParseParameters(string input, IDictionary<string, object> environment, char seperateListItemsWith = '\n')
        {
            return Task.Factory.StartNew(() =>
            {
                input = input ?? "";

                return ParameterRegEx.Replace(input, x =>
                {
                    var value = FindParameterValue(x.Groups["expression"].Value, environment).Result;

                    if (value == null) return "";

                    if (value is IEnumerable && !(value is string))
                    {
                        var stringValues = ((IEnumerable) value).OfType<object>().Select(y => y.ToString()).ToList();

                        return string.Join(seperateListItemsWith.ToString(CultureInfo.InvariantCulture), stringValues);
                    }

                    return value.ToString();
                });
            });
        }


        public async Task<object> FindParameterValue(string parameter, IDictionary<string, object> environment)
        {
            object value = null;
            parameter = parameter ?? "";

            foreach (var findParameterValue in _findParameterValues)
            {
                value = await findParameterValue.Find(parameter, environment);

                if (value != null) break;
            }

            return value;
        }

        public async Task<bool> FindBoolValue(string parameter, IDictionary<string, object> environment, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(parameter))
                return defaultValue;

            return ((await FindParameterValue(parameter, environment)) as bool?).GetValueOrDefault(defaultValue);
        }
    }
}