using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jajo.Web.ModelBinding
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class BindModels
    {
        private readonly AppFunc _next;
        private readonly IModelBinderCollection _modelBinderCollection;

        public BindModels(AppFunc next, IModelBinderCollection modelBinderCollection)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _modelBinderCollection = modelBinderCollection;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment["jajo.ModelBinder"] = (Func<Type, object>)(x => _modelBinderCollection.Bind(x, new BindingContext(_modelBinderCollection)));

            await _next(environment);
        }
    }
}