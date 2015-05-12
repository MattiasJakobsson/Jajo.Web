using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jajo.Web.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RollbackUnitOfWork
    {
        private readonly AppFunc _next;

        public RollbackUnitOfWork(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var unitOfWorks = environment.ResolveAll<IJajoUnitOfWork>().ToList();

            var exception = environment.GetException();

            foreach (var unitOfWork in unitOfWorks)
                unitOfWork.Rollback(exception);

            await _next(environment);
        }
    }
}