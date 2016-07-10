using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.Web.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateRequest
    {
        private readonly AppFunc _next;

        public ValidateRequest(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var errors = new List<ValidationResult.ValidationError>();

            var validators = environment.ResolveAll<IValidateRequest>();

            foreach (var validator in validators)
            {
                var result = await validator.Validate(environment).ConfigureAwait(false);
                errors.AddRange(result.Errors);
            }

            var validationResult = new ValidationResult(errors);

            if (validationResult.IsValid)
                await _next(environment).ConfigureAwait(false);
            else
                environment[ValidationEnvironmentExtensions.ValidationConstants.ValidationResult] = validationResult;
        }
    }
}