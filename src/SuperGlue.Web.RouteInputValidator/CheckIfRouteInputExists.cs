using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Routing;

namespace SuperGlue.Web.RouteInputValidator
{
    public class CheckIfRouteInputExists : ICheckIfRouteExists
    {
        private static readonly Cache<Type, Func<object, object, Task<bool>>> EnsureExistsExecutionMethods = new Cache<Type, Func<object, object, Task<bool>>>(); 

        public async Task<bool> Exists(object routeEndpoint, IDictionary<string, object> environment)
        {
            var inputTypes = environment
                .GetInputsForEndpoint(routeEndpoint);

            foreach (var inputType in inputTypes)
            {
                var currentInputType = inputType;

                var input = await environment.Bind(currentInputType).ConfigureAwait(false);

                var existenceCheckers = environment.ResolveAll(typeof(IEnsureExists<>).MakeGenericType(currentInputType));

                foreach (var existenceChecker in existenceCheckers)
                {
                    var currentExistenceChecker = existenceChecker;

                    if (!await EnsureExistsExecutionMethods.Get(existenceChecker.GetType(), x => CompileExecutionFunctionFor(currentExistenceChecker.GetType(), currentInputType))(existenceChecker, input).ConfigureAwait(false))
                        return false;
                }
            }

            return true;
        }

        protected virtual Func<object, object, Task<bool>> CompileExecutionFunctionFor(Type ensureExistsType, Type inputType)
        {
            return (Func<object, object, Task<bool>>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(ensureExistsType, inputType)
                .Invoke(this, new object[0]);
        }

        protected virtual Func<object, object, Task<bool>> CompileExecutionFunctionForGeneric<TEnsureExists, TInput>() where TEnsureExists : IEnsureExists<TInput>
        {
            var ensureExistsType = typeof(TEnsureExists);
            var inputType = typeof(TInput);

            var method = ensureExistsType.GetMethod("Exists", new[] { inputType });

            var ensureExistsParameter = Expression.Parameter(ensureExistsType);
            var inputParameter = Expression.Parameter(inputType);

            var execute = Expression
                .Lambda<Func<TEnsureExists, TInput, Task<bool>>>(Expression.Call(ensureExistsParameter, method, inputParameter), ensureExistsParameter, inputParameter)
                .Compile();

            return ((ensureExists, input) => execute((TEnsureExists)ensureExists, (TInput)input));
        }
    }
}