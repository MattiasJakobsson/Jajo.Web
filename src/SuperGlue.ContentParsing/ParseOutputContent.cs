using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Web.Output;

namespace SuperGlue.ContentParsing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ParseOutputContent
    {
        private readonly AppFunc _next;

        public ParseOutputContent(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var output = environment.GetOutputResult();

            if (output != null)
            {
                var parsers = environment.ResolveAll<ITextParser>();

                var text = await parsers.ParseText(environment, output.Body).ConfigureAwait(false);

                environment.SetOutputResult(new OutputRenderingResult(text, output.ContentType));
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}