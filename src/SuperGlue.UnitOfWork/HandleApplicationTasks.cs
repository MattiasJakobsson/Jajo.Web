using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.UnitOfWork
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HandleApplicationTasks
    {
        private readonly AppFunc _next;

        public HandleApplicationTasks(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var mode = environment.GetMode();

            if (mode == SetupMode.StartUp)
            {
                var applicationTasks = environment.ResolveAll<IApplicationTask>();

                foreach (var applicationTask in applicationTasks)
                    await applicationTask.Start();
            }
            else if (mode == SetupMode.ShutDown)
            {
                var applicationTasks = environment.ResolveAll<IApplicationTask>();

                foreach (var applicationTask in applicationTasks)
                    await applicationTask.ShutDown();
            }

            await _next(environment);
        }
    }
}