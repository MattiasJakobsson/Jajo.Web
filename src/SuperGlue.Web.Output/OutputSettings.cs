using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.Web.Output
{
    public class OutputSettings
    {
        private readonly ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> _renderers = new List<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>();

        public OutputSettings AddRenderer(Func<IDictionary<string, object>, bool> match, IRenderOutput renderer)
        {
            _renderers.Add(new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(match, renderer));

            return this;
        }

        internal IRenderOutput FindRenderer(IDictionary<string, object> environment)
        {
            return _renderers.Where(x => x.Item1(environment))
                .Select(x => x.Item2)
                .LastOrDefault();
        }
    }
}