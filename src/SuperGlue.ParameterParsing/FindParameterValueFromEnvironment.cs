using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.ParameterParsing
{
    public class FindParameterValueFromEnvironment : IFindParameterValue
    {
        private readonly IParseExpressionFor _parseExpressionFor;

        public FindParameterValueFromEnvironment(IParseExpressionFor parseExpressionFor)
        {
            _parseExpressionFor = parseExpressionFor;
        }

        public Task<object> Find(string parameter, IDictionary<string, object> environment)
        {
            return Task.FromResult(environment
                .Where(item => parameter.StartsWith(string.Format("{0}.", item.Key)))
                .Select(item => _parseExpressionFor.Parse(parameter.Substring(string.Format("{0}.", item.Key).Length), item.Value))
                .FirstOrDefault());
        }
    }
}