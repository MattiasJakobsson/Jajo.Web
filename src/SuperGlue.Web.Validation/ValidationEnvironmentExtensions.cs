using System.Collections.Generic;

namespace SuperGlue.Web.Validation
{
    public static class ValidationEnvironmentExtensions
    {
        public static class ValidationConstants
        {
            public const string ValidationResult = "superglue.ValidationResult";
        }

        public static ValidationResult GetValidationResult(this IDictionary<string, object> environment)
        {
            return environment.Get(ValidationConstants.ValidationResult, new ValidationResult(new List<ValidationResult.ValidationError>()));
        }
    }
}