using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    public static class OutputEnvironmentExtensions
    {
        public static class OutputConstants
        {
            public const string Renderers = "superglue.OutputRenderers";
        }

        public static async Task Render(this IDictionary<string, object> environment)
        {
            var renderers = environment.Get<ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>>(OutputConstants.Renderers, new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>());

            var renderer = renderers.FirstOrDefault(x => x.Item1(environment));

            if (renderer == null)
                return;

            var result = await renderer.Item2.Render(environment);

            if (result == null)
                return;

            var response = environment.GetResponse();

            response.Headers.ContentType = result.ContentType;

            await response.Write(result.Body);
        }

        internal static void AddRenderer(this IDictionary<string, object> environment, Func<IDictionary<string, object>, bool> match, IRenderOutput renderer)
        {
            var renderers = environment.Get<ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>>(OutputConstants.Renderers);

            if (renderers == null)
            {
                renderers = new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>();

                environment[OutputConstants.Renderers] = renderers;
            }

            renderers.Add(new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(match, renderer));
        }
    }
}