using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWeb.Output
{
    public class HandleOutputRendering : IHandleOutputRendering
    {
        private readonly IEnumerable<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> _outputRenderers;

        public HandleOutputRendering(IEnumerable<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> outputRenderers)
        {
            _outputRenderers = outputRenderers;
        }

        public async Task Render(IDictionary<string, object> environment)
        {
            var renderer = _outputRenderers.FirstOrDefault(x => x.Item1(environment));

            if (renderer == null)
                return;

            await environment.WriteToOutput(await renderer.Item2.Render(environment));
        }
    }
}