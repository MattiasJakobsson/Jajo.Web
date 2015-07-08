using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Logging;

namespace SuperGlue.ExceptionManagement
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleExceptions
    {
        private readonly AppFunc _next;

        public HandleExceptions(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                await _next(environment);
            }
            catch (Exception ex)
            {
                environment[ExceptionExtensions.ExceptionConstants.Exception] = ex;

                var log = environment.Resolve<ILog>();

                log.Error(ex, "Exception while executing chain: {0}", environment.GetCurrentChain());
            }
        }
    }
}