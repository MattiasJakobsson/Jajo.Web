using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenWeb.Output
{
    public class OutputRendererBuilder
    {
        private readonly ICollection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>> _outputRenderers = new Collection<Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>>();

        private OutputRendererBuilder()
        {

        }

        public static OutputRendererBuilder New()
        {
            return new OutputRendererBuilder();
        }

        public WhenConfiguration When(Func<IDictionary<string, object>, bool> when)
        {
            return new WhenConfiguration(when, this);
        }

        public IHandleOutputRendering Build()
        {
            return new HandleOutputRendering(_outputRenderers);
        }

        public class WhenConfiguration
        {
            private readonly Func<IDictionary<string, object>, bool> _when;
            private readonly OutputRendererBuilder _rendererBuilder;

            public WhenConfiguration(Func<IDictionary<string, object>, bool> when, OutputRendererBuilder rendererBuilder)
            {
                _when = when;
                _rendererBuilder = rendererBuilder;
            }

            public OutputRendererBuilder UseRenderer(IRenderOutput renderOutput)
            {
                _rendererBuilder._outputRenderers.Add(new Tuple<Func<IDictionary<string, object>, bool>, IRenderOutput>(_when, renderOutput));

                return _rendererBuilder;
            }
        }
    }
}