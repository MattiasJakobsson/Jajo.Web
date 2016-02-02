namespace SuperGlue.ContentParsing
{
    public class ModelExpressionParseResult
    {
        public ModelExpressionParseResult(bool success, object result)
        {
            Result = result;
            Success = success;
        }

        public bool Success { get; private set; }
        public object Result { get; private set; }
    }
}