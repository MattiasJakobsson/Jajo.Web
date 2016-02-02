using System.Collections.Generic;

namespace SuperGlue.Web.Output
{
    public static class OutputExtensions
    {
        public class OutputConstants
        {
            public const string OutputResult = "superglue.OutputResult";
        }

        public static void SetOutputResult(this IDictionary<string, object> environment, OutputRenderingResult result)
        {
            environment[OutputConstants.OutputResult] = result;
        }

        public static OutputRenderingResult GetOutputResult(this IDictionary<string, object> environment)
        {
            return environment.Get<OutputRenderingResult>(OutputConstants.OutputResult);
        }
    }
}