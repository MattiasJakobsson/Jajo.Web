using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.ContentParsing
{
    public class DefaultParameterValueFinder : IFindParameterValueFromModel
    {
        private readonly IEnumerable<IParseModelExpression> _expressionParts;

        public DefaultParameterValueFinder(IEnumerable<IParseModelExpression> expressionParts)
        {
            _expressionParts = expressionParts;
        }

        public object Find(string expression, object model)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return null;

            var parts = expression.Split('.').ToList();

            var currentModel = model;

            foreach (var part in parts)
            {
                if (currentModel == null)
                    return null;

                var firstSuccessfullPart = _expressionParts.Select(x =>
                {
                    var result = x.Parse(part, currentModel);

                    return result.Success ? result : null;
                }).FirstOrDefault(x => x != null);

                if (firstSuccessfullPart == null)
                    return null;

                currentModel = firstSuccessfullPart.Result;
            }

            return currentModel;
        }
    }
}