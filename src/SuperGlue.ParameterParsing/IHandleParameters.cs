using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.ParameterParsing
{
    public interface IHandleParameters
    {
        Task<string> ParseParameters(string input, IDictionary<string, object> environment, char seperateListItemsWith = '\n');
        Task<object> FindParameterValue(string parameter, IDictionary<string, object> environment);
        Task<bool> FindBoolValue(string parameter, IDictionary<string, object> environment, bool defaultValue = false);
    }
}