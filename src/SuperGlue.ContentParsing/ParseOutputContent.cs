using System;
using System.Collections.Generic;
using System.Linq;
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

                var text = await ParseText(environment, output.Body, parsers).ConfigureAwait(false);

                environment.SetOutputResult(new OutputRenderingResult(text, output.ContentType));
            }

            await _next(environment).ConfigureAwait(false);
        }

        private static async Task<string> ParseText(IDictionary<string, object> environment, string text, IEnumerable<ITextParser> parsers)
        {
            var parserList = parsers.ToList();

            foreach (var parser in parserList)
                text = await parser.Parse(environment, text, x => ParseText(environment, x, parserList)).ConfigureAwait(false);

            return text;
        }
    }
}