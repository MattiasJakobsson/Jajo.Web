using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public class HandleOutputRendering : IHandleOutputRendering
    {
        private readonly ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> _outputRenderers;

        public HandleOutputRendering(ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> outputRenderers)
        {
            _outputRenderers = outputRenderers;
        }

        public async Task Render(IDictionary<string, object> environment)
        {
            var renderer = _outputRenderers.FirstOrDefault(x => x.Item1(environment));

            if (renderer == null)
                return;

            var result = await renderer.Item2.Render(environment);

            if (result == null)
                return;

            var response = environment.GetResponse();

            response.Headers.ContentType = result.ContentType;

            await response.Write(result.Body);
        }
    }
}