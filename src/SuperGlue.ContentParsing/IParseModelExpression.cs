namespace SuperGlue.ContentParsing
{
    public interface IParseModelExpression
    {
        ModelExpressionParseResult Parse(string expression, object model);
    }
}