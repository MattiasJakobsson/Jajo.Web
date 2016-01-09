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

        public HandleUnitOfWork(AppFunc next, HandleUnitOfWorkOptions options)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _next = next;
            _handleRollback = options.HandleRollback;
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

    public class HandleUnitOfWorkOptions
    {
        public HandleUnitOfWorkOptions(bool handleRollback = false)
        {
            HandleRollback = handleRollback;
        }

        public bool HandleRollback { get; } 
    }
}