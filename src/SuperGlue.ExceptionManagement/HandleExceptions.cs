using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                environment["superglue.Exception"] = ex;
            }
        }
    }
}