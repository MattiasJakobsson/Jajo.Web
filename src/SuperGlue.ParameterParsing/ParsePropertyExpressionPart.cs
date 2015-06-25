namespace SuperGlue.ParameterParsing
{
    public class ParsePropertyExpressionPart : IParseExpressionPart
    {
        public ExpressionParseResult Parse(string part, object model)
        {
            if (model == null)
                return new ExpressionParseResult(false, null);

            var property = model.GetType().GetProperty(part);

            if (property == null)
                return new ExpressionParseResult(false, null);

            return new ExpressionParseResult(true, property.GetValue(model));
        }
    }
}