using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.ModelBinding
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebModelBindingMiddleware
    {
        private readonly AppFunc _next;
        private readonly IModelBinderCollection _modelBinderCollection;

        public OpenWebModelBindingMiddleware(AppFunc next, IModelBinderCollection modelBinderCollection)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _modelBinderCollection = modelBinderCollection;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment.SetModelBinder(_modelBinderCollection);

            await _next(environment);
        }
    }
}