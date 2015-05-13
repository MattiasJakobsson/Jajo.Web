using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Web.Validation.InputValidation
{
    public class ValidateRequestInput : IValidateRequest
    {
        public ValidationResult Validate(IDictionary<string, object> environment)
        {
            var methodEndpoint = environment.GetRouteInformation().RoutedTo as MethodInfo;

            if (methodEndpoint == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var inputType = methodEndpoint.GetParameters().Select(x => x.ParameterType).FirstOrDefault();

            if (inputType == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var input = environment.Bind(inputType);

            if (input == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var validator = environment.Resolve(typeof(IValidateInput<>).MakeGenericType(input.GetType()));

            if (validator == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            return (ValidationResult)validator
                .GetType()
                .GetMethod("Validate", new[] { input.GetType() })
                .Invoke(validator, new[] { input });
        }
    }
}