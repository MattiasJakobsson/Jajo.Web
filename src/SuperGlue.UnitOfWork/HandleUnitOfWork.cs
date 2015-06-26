using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleUnitOfWork
    {
        private readonly AppFunc _next;

        public HandleUnitOfWork(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var unitOfWorks = environment.ResolveAll<ISuperGlueUnitOfWork>().ToList();

            foreach (var unitOfWork in unitOfWorks)
                await unitOfWork.Begin();

            await _next(environment);

            foreach (var unitOfWork in unitOfWorks)
                await unitOfWork.Commit();
        }
    }
}