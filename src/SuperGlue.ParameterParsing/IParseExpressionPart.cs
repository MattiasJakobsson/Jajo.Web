namespace SuperGlue.ParameterParsing
{
    public interface IParseExpressionPart
    {
        ExpressionParseResult Parse(string part, object model);
    }
}