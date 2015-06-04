using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Output
{
    public static class OutputSettingsExtensions
    {
        public static RendererConfigurer When(this OutputSettings settings, Func<IDictionary<string, object>, bool> match)
        {
            return new RendererConfigurer(match, settings);
        }

        public class RendererConfigurer
        {
            private readonly Func<IDictionary<string, object>, bool> _match;
            private readonly OutputSettings _settings;

            public RendererConfigurer(Func<IDictionary<string, object>, bool> match, OutputSettings settings)
            {
                _match = match;
                _settings = settings;
            }

            public OutputSettings UseRenderer(IRenderOutput renderer)
            {
                _settings.AddRenderer(_match, renderer);

                return _settings;
            }
        }
    }
}