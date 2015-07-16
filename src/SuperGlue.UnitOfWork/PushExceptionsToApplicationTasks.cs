using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.ExceptionManagement;

namespace SuperGlue.UnitOfWork
{
    public class PushExceptionsToApplicationTasks : IWrapMiddleware<HandleExceptions>
    {
        public Task<IEndThings> Begin(IDictionary<string, object> environment, Type middlewareType)
        {
            return Task.FromResult<IEndThings>(new Disposable(environment));
        }

        private class Disposable : IEndThings
        {
            private readonly IDictionary<string, object> _environment;

            public Disposable(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public Task End()
            {
                var exception = _environment.GetException();

                if (exception == null)
                    return Task.CompletedTask;

                var applicationTasks = _environment.ResolveAll<IApplicationTask>();

                var actions = applicationTasks.Select(applicationTask => applicationTask.Exception(exception)).ToList();

                return Task.WhenAll(actions);
            }
        }
    }
}