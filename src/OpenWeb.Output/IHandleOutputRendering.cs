using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWeb.Output
{
    public interface IHandleOutputRendering
    {
        Task Render(WebEnvironment environment, Stream body);
    }

    public class HandleOutputRendering : IHandleOutputRendering
    {
        private readonly IEnumerable<Tuple<Func<WebEnvironment, bool>, IRenderOutput>> _outputRenderers;

        public HandleOutputRendering(IEnumerable<Tuple<Func<WebEnvironment, bool>, IRenderOutput>> outputRenderers)
        {
            _outputRenderers = outputRenderers;
        }

        public async Task Render(WebEnvironment environment, Stream body)
        {
            var renderer = _outputRenderers.FirstOrDefault(x => x.Item1(environment));

            if (renderer == null)
                return;

            using (var output = await renderer.Item2.Render(environment))
            {
                output.Position = 0;

                await output.CopyToAsync(body);   
            }
        }
    }
}