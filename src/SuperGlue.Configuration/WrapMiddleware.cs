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
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var wrappers = new List<IDisposable>();

            foreach (var wrapper in _options.Wrappers)
                wrappers.Add(await wrapper.Begin(environment, _options.MiddleWareType));

            await _next(environment);

            foreach (var wrapper in wrappers)
                wrapper.Dispose();
        }
    }

    public class WrapMiddlewareOptions<TMiddleware>
    {
        public WrapMiddlewareOptions(IEnumerable<IWrapMiddleware<TMiddleware>> wrappers, Type middleWareType)
        {
            Wrappers = wrappers;
            MiddleWareType = middleWareType;
        }

        public IEnumerable<IWrapMiddleware<TMiddleware>> Wrappers { get; private set; }
        public Type MiddleWareType { get; private set; }
    }
}