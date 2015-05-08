using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenWeb.Endpoints;
using OpenWeb.ModelBinding;

namespace OpenWeb.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebUnitOfWorkMiddleware
    {
        private readonly AppFunc _next;
        private readonly IModelBinderCollection _modelBinderCollection;

        public OpenWebUnitOfWorkMiddleware(AppFunc next, IModelBinderCollection modelBinderCollection)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _modelBinderCollection = modelBinderCollection;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OpenWebContext(environment, _modelBinderCollection);

            var unitOfWorks = context.DependencyResolver.ResolveAll<IOpenWebUnitOfWork>().ToList();

            foreach (var unitOfWork in unitOfWorks)
                unitOfWork.Begin();

            await _next(environment);

            foreach (var unitOfWork in unitOfWorks)
                unitOfWork.Commit();
        } 
    }
}