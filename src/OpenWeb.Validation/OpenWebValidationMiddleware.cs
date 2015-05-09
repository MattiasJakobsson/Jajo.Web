using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWeb.Validation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OpenWebValidationMiddleware
    {
        private readonly AppFunc _next;
        private readonly OpenWebValidationOptions _options;

        public OpenWebValidationMiddleware(AppFunc next, OpenWebValidationOptions options)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var validationResults = _options.Validators.Select(x => x.Validate(environment)).ToList();

            var errors = new List<ValidationResult.ValidationError>();
            errors.AddRange(validationResults.SelectMany(x => x.Errors));

            var validationResult = new ValidationResult(errors);

            if (validationResult.IsValid)
                await _next(environment);
            else
                environment["openweb.ValidationResult"] = validationResult;
        }
    }

    public class OpenWebValidationOptions
    {
        private readonly IList<IValidateRequest> _validators = new List<IValidateRequest>();

        public IReadOnlyCollection<IValidateRequest> Validators { get { return new ReadOnlyCollection<IValidateRequest>(_validators); } }

        public OpenWebValidationOptions UsingValidator(IValidateRequest validator)
        {
            _validators.Add(validator);

            return this;
        }
    }
}