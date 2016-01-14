using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.ExceptionManagement
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleExceptions
    {
        private readonly AppFunc _next;

        public HandleExceptions(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                await _next(environment).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                environment[ExceptionExtensions.ExceptionConstants.Exception] = ex;

                environment.Log(ex, "Exception while executing chain: {0}", LogLevel.Error, environment.GetCurrentChain().Name);
            }
        }
    }
}