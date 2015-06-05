using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RenderOutput
    {
        private readonly AppFunc _next;

        public RenderOutput(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.Resolve<IRenderToOutput>().Render(environment);

            await _next(environment);
        }
    }
}