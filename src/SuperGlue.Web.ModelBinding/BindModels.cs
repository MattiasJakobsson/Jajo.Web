using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.ModelBinding
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class BindModels
    {
        private readonly AppFunc _next;

        public BindModels(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var modelBinderCollection = environment.Resolve<IModelBinderCollection>();

            environment[BindExtensions.BindConstants.ModelBinder] = (Func<Type, Task<object>>)(async x =>
            {
                var result = await modelBinderCollection.Bind(x, new BindingContext(modelBinderCollection, environment)).ConfigureAwait(false);

                return result.Instance;
            });

            await _next(environment).ConfigureAwait(false);
        }
    }
}