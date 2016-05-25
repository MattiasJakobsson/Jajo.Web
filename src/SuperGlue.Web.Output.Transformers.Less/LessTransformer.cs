using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output.Transformers.Less
{
    public class LessTransformer : ITransformOutput
    {
        public async Task<OutputRenderingResult> Transform(OutputRenderingResult result, IDictionary<string, object> environment)
        {
            var filePath = environment.GetRequest().Path.Split('?').FirstOrDefault() ?? "";

            //TODO:Make configurable
            if (!filePath.EndsWith(".less.css"))
            {
                environment.Log($"Skipping less transformation for file: {filePath}", LogLevel.Debug);
                return result;
            }

            environment.Log($"Transforming file: {filePath} using less", LogLevel.Debug);

            result.Body.Position = 0;
            using (var reader = new StreamReader(result.Body))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                content = dotless.Core.Less.Parse(content);

                return new OutputRenderingResult(await content.ToStream().ConfigureAwait(false), "text/css");
            }
        }
    }
}