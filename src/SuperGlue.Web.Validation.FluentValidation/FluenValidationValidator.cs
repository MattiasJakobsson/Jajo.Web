using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using SuperGlue.Web.Validation.InputValidation;

namespace SuperGlue.Web.Validation.FluentValidation
{
    public class FluenValidationValidator<TInput> : AbstractValidator<TInput>, IInputValidator
    {
        private readonly TInput _input;

        public FluenValidationValidator(TInput input)
        {
            _input = input;
        }

        public async Task<ValidationResult> Validate()
        {
            var validationResult = await ValidateAsync(_input).ConfigureAwait(false);
        
            return new ValidationResult(validationResult.Errors.Select(x => new ValidationResult.ValidationError(x.PropertyName, x.ErrorMessage)));
        }
    }
}