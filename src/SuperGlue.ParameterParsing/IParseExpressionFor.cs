namespace SuperGlue.ParameterParsing
{
    public interface IParseExpressionFor
    {
        object Parse(string expression, object model);
    }
}