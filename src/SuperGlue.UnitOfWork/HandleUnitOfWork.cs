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
                await unitOfWork.Begin().ConfigureAwait(false);

            if (_handleRollback)
            {
                try
                {
                    await _next(environment).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    foreach (var unitOfWork in unitOfWorks)
                        await unitOfWork.Rollback(ex).ConfigureAwait(false);

                    throw;
                }
            }
            else
            {
                await _next(environment).ConfigureAwait(false);
            }

            foreach (var unitOfWork in unitOfWorks)
                await unitOfWork.Commit().ConfigureAwait(false);
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