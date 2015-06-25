namespace SuperGlue.ParameterParsing
{
    public class ExpressionParseResult
    {
        public ExpressionParseResult(bool success, object result)
        {
            Result = result;
            Success = success;
        }

        public bool Success { get; private set; }
        public object Result { get; private set; }
    }
}