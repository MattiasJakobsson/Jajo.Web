using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Validation.InputValidation
{
    public class ValidateRequestInput : IValidateRequest
    {
        private static readonly Cache<Type, Func<object, object, ValidationResult>> ValidationHandlersCache = new Cache<Type, Func<object, object, ValidationResult>>(
            x =>
            {
                var validateInputType = typeof (IValidateInput<>).MakeGenericType(x);

                return (Func<object, object, ValidationResult>) typeof (ValidateRequestInput)
                    .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(validateInputType, x)
                    .Invoke(null, new object[0]);
            });

        public async Task<ValidationResult> Validate(IDictionary<string, object> environment)
        {
            var methodEndpoint = environment.GetRouteInformation().RoutedTo as MethodInfo;

            if (methodEndpoint == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var inputType = methodEndpoint.GetParameters().Select(x => x.ParameterType).FirstOrDefault();

            if (inputType == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var input = await environment.Bind(inputType);

            if (input == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var validator = environment.Resolve(typeof(IValidateInput<>).MakeGenericType(input.GetType()));

            if (validator == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            return ValidationHandlersCache[input.GetType()](validator, input);
        }

        protected static Func<object, object, ValidationResult> CompileExecutionFunctionForGeneric<TValidator, TInput>() where TValidator : IValidateInput<TInput>
        {
            var validatorType = typeof(TValidator);
            var inputType = typeof(TInput);

            var method = validatorType.GetMethod("Validate", new[] { inputType });

            var validatorParameter = Expression.Parameter(validatorType);
            var inputParameter = Expression.Parameter(inputType);

            var execute = Expression
                .Lambda<Func<TValidator, TInput, ValidationResult>>(Expression.Call(validatorParameter, method, inputParameter), validatorParameter, inputParameter)
                .Compile();

            return ((validator, input) => execute((TValidator)validator, (TInput)input));
        }
    }
}