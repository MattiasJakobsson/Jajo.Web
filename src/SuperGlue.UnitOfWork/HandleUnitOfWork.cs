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
        private readonly bool _handleRollback;

        public HandleUnitOfWork(AppFunc next, bool handleRollback = false)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _handleRollback = handleRollback;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var unitOfWorks = environment.ResolveAll<ISuperGlueUnitOfWork>().ToList();

            foreach (var unitOfWork in unitOfWorks)
                await unitOfWork.Begin();

            if (_handleRollback)
            {
                try
                {
                    await _next(environment);
                }
                catch (Exception ex)
                {
                    foreach (var unitOfWork in unitOfWorks)
                        await unitOfWork.Rollback(ex);

                    throw;
                }
            }
            else
            {
                await _next(environment);
            }

            foreach (var unitOfWork in unitOfWorks)
                await unitOfWork.Commit();
        }
    }
}