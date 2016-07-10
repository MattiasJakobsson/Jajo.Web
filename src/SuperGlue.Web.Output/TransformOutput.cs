using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class TransformOutput
    {
        private readonly AppFunc _next;

        public TransformOutput(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var transformers = environment.ResolveAll<ITransformOutput>().ToList();

            var output = environment.GetOutputResult();

            if (output == null)
                environment.Log("Skipping transformers as we don't have any output", LogLevel.Debug);

            if (output != null)
            {
                environment.Log($"Going to transform output using {transformers.Count} transformers", LogLevel.Debug);

                foreach (var transformer in transformers)
                {
                    output = await transformer.Transform(output, environment).ConfigureAwait(false);
                    environment.Log($"Finished transforming using transformer {transformer.GetType().FullName}", LogLevel.Debug);
                }

                environment.SetOutputResult(output);
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}