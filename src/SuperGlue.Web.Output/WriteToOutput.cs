using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Output
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class WriteToOutput
    {
        private readonly AppFunc _next;

        public WriteToOutput(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await environment.Resolve<IWriteToOutput>().Write(environment, environment.GetOutputResult()).ConfigureAwait(false);

            await _next(environment).ConfigureAwait(false);
        }
    }
}