namespace SuperGlue.ContentParsing
{
    public class ParsePropertyExpressionPart : IParseModelExpression
    {
        public ModelExpressionParseResult Parse(string part, object model)
        {
            if (model == null)
                return new ModelExpressionParseResult(false, null);

            var property = model.GetType().GetProperty(part);

            return property == null ? new ModelExpressionParseResult(false, null) : new ModelExpressionParseResult(true, property.GetValue(model));
        }
    }
}