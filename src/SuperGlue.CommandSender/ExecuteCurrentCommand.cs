using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteCurrentCommand
    {
        private readonly AppFunc _next;
        private readonly Cache<Type, Func<object, object, IDictionary<string, object>, Task>> _commandExecutionMethods = new Cache<Type, Func<object, object, IDictionary<string, object>, Task>>();

        public ExecuteCurrentCommand(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var command = environment.GetCurrentCommand();

            if (command != null)
            {
                var executor = environment.Resolve(typeof(IHandleCommand<>).MakeGenericType(command.GetType()));

                if (executor != null)
                {
                    var executionMethod = _commandExecutionMethods.Get(command.GetType(), key => CompileExecutionFunctionFor(executor.GetType(), command.GetType()));

                    await executionMethod(executor, command, environment);
                }
            }

            await _next(environment);
        }

        protected Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionFor(Type executorType, Type commandType)
        {
            return (Func<object, object, IDictionary<string, object>, Task>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(executorType, commandType)
                .Invoke(this, new object[0]);
        }

        protected Func<object, object, IDictionary<string, object>, Task> CompileExecutionFunctionForGeneric<TExecutor, TCommand>()
        {
            var executorType = typeof(TExecutor);
            var commandType = typeof(TCommand);

            var method = executorType.GetMethod("Handle", new[] { commandType, typeof(IDictionary<string, object>) });

            var executorParameter = Expression.Parameter(executorType);
            var commandParameter = Expression.Parameter(commandType);
            var environmentParameter = Expression.Parameter(typeof(IDictionary<string, object>));

            var execute = Expression
                .Lambda<Func<TExecutor, TCommand, IDictionary<string, object>, Task>>(Expression.Call(executorParameter, method, commandParameter, environmentParameter), executorParameter, commandParameter, environmentParameter)
                .Compile();

            return ((executor, command, environment) => execute((TExecutor)executor, (TCommand)command, environment));
        }
    }
}