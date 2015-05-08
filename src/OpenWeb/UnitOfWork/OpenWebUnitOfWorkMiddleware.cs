using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWeb.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebUnitOfWorkMiddleware
    {
        private readonly AppFunc _next;

        public OpenWebUnitOfWorkMiddleware(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var unitOfWorks = environment.GetDependencyResolver().ResolveAll<IOpenWebUnitOfWork>().ToList();

            foreach (var unitOfWork in unitOfWorks)
                unitOfWork.Begin();

            await _next(environment);

            foreach (var unitOfWork in unitOfWorks)
                unitOfWork.Commit();
        } 
    }
}