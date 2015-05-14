using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SuperGlue.Web.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ValidateRequest
    {
        private readonly AppFunc _next;
        private readonly ValidateRequestOptions _options;

        public ValidateRequest(AppFunc next, ValidateRequestOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var errors = new List<ValidationResult.ValidationError>();

            foreach (var validator in _options.Validators)
            {
                var result = await validator.Validate(environment);
                errors.AddRange(result.Errors);
            }

            var validationResult = new ValidationResult(errors);

            if (validationResult.IsValid)
                await _next(environment);
            else
                environment["superglue.ValidationResult"] = validationResult;
        }
    }

    public class ValidateRequestOptions
    {
        private readonly IList<IValidateRequest> _validators = new List<IValidateRequest>();

        public IReadOnlyCollection<IValidateRequest> Validators { get { return new ReadOnlyCollection<IValidateRequest>(_validators); } }

        public ValidateRequestOptions UsingValidator(IValidateRequest validator)
        {
            _validators.Add(validator);

            return this;
        }
    }
}