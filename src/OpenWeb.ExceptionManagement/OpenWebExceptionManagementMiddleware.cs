using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.ExceptionManagement
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebExceptionManagementMiddleware
    {
        private readonly AppFunc _next;

        public OpenWebExceptionManagementMiddleware(AppFunc next)
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
                //TODO:Handle exception correctly
            }
        }
    }
}