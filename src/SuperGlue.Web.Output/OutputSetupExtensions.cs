using System;
using System.Collections.Generic;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    public static class OutputSetupExtensions
    {
        public static OutputConfiguration ConfigureOutput(this SuperGlueBootstrapper bootstrapper, IDictionary<string, object> environment)
        {
            return new OutputConfiguration(environment);
        }

        public class OutputConfiguration
        {
            private readonly IDictionary<string, object> _environment;

            public OutputConfiguration(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public RendererConfigurer When(Func<IDictionary<string, object>, bool> match)
            {
                return new RendererConfigurer(_environment, match);
            }
        }

        public class RendererConfigurer
        {
            private readonly Func<IDictionary<string, object>, bool> _match;

            public RendererConfigurer(IDictionary<string, object> environment, Func<IDictionary<string, object>, bool> match)
            {
                _match = match;
                Environment = environment;
            }

            public IDictionary<string, object> Environment { get; private set; }

            public OutputConfiguration UseRenderer(IRenderOutput renderer)
            {
                Environment.AddRenderer(_match, renderer);

                return new OutputConfiguration(Environment);
            }
        }
    }
}