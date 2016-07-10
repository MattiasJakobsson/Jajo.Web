using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.CommandSender
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteCurrentCommand
    {
        private readonly AppFunc _next;
        private readonly Cache<Type, Func<object, object, Task>> _commandExecutionMethods = new Cache<Type, Func<object, object, Task>>();

        public ExecuteCurrentCommand(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var command = environment.GetCurrentCommand()?.CommandObject;

            if (command != null)
            {
                var executor = environment.Resolve(typeof(IHandleCommand<>).MakeGenericType(command.GetType()));

                if (executor != null)
                {
                    var executionMethod = _commandExecutionMethods.Get(command.GetType(), key => CompileExecutionFunctionFor(executor.GetType(), command.GetType()));

                    await executionMethod(executor, command).ConfigureAwait(false);
                }
            }

            await _next(environment).ConfigureAwait(false);
        }

        protected Func<object, object, Task> CompileExecutionFunctionFor(Type executorType, Type commandType)
        {
            return (Func<object, object, Task>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(executorType, commandType)
                .Invoke(this, new object[0]);
        }

        protected Func<object, object, Task> CompileExecutionFunctionForGeneric<TExecutor, TCommand>()
        {
            var executorType = typeof(TExecutor);
            var commandType = typeof(TCommand);

            var method = executorType.GetMethod("Handle", new[] { commandType });

            var executorParameter = Expression.Parameter(executorType);
            var commandParameter = Expression.Parameter(commandType);

            var execute = Expression
                .Lambda<Func<TExecutor, TCommand, Task>>(Expression.Call(executorParameter, method, commandParameter), executorParameter, commandParameter)
                .Compile();

            return (executor, command) => execute((TExecutor)executor, (TCommand)command);
        }
    }
}