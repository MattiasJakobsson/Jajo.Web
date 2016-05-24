using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output.Transformers.Less
{
    public class LessTransformer : ITransformOutput
    {
        public Task<OutputRenderingResult> Transform(OutputRenderingResult result, IDictionary<string, object> environment)
        {
            var filePath = environment.GetRequest().Path.Split('?').FirstOrDefault() ?? "";

            //TODO:Make configurable
            if (!filePath.EndsWith(".less.css"))
            {
                environment.Log($"Skipping less transformation for file: {filePath}", LogLevel.Debug);
                return Task.FromResult(result);
            }

            environment.Log($"Transforming file: {filePath} using less", LogLevel.Debug);

            return Task.FromResult(new OutputRenderingResult(dotless.Core.Less.Parse(result.Body), "text/css"));
        }
    }
}