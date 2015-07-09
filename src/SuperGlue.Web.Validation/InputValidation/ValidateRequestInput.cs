using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperGlue.Web.Validation.InputValidation
{
    public class ValidateRequestInput : IValidateRequest
    {
        public async Task<ValidationResult> Validate(IDictionary<string, object> environment)
        {
            var methodEndpoint = environment.GetRouteInformation().RoutedTo as MethodInfo;

            if (methodEndpoint == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var inputType = methodEndpoint.GetParameters().Select(x => x.ParameterType).FirstOrDefault();

            if (inputType == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var input = await environment.Bind(inputType) as IAmValidatedBy;

            if (input == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            var validator = input.GetValidator(environment);

            if (validator == null)
                return new ValidationResult(new List<ValidationResult.ValidationError>());

            return await validator.Validate();
        }
    }
}