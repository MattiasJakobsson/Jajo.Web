using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class WrapMiddleware<TMiddleware>
    {
        private readonly AppFunc _next;
        private readonly WrapMiddlewareOptions<TMiddleware> _options;

        public WrapMiddleware(AppFunc next, WrapMiddlewareOptions<TMiddleware> options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var wrapper = await _options.Wrapper.Begin(environment, _options.MiddleWareType);

            await _next(environment);

            await wrapper.End();
        }
    }

    public class WrapMiddlewareOptions<TMiddleware>
    {
        public WrapMiddlewareOptions(IWrapMiddleware<TMiddleware> wrapper, Type middleWareType)
        {
            MiddleWareType = middleWareType;
            Wrapper = wrapper;
        }

        public IWrapMiddleware<TMiddleware> Wrapper { get; }
        public Type MiddleWareType { get; }
    }
}