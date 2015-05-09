using System.Collections.Generic;
using System.Linq;

namespace OpenWeb.Validation
{
    public class ValidationResult
    {
        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Errors = errors;
        }

        public bool IsValid { get { return !Errors.Any(); } }
        public IEnumerable<ValidationError> Errors { get; private set; }

        public class ValidationError
        {
            public ValidationError(string key, string message)
            {
                Key = key;
                Message = message;
            }

            public string Key { get; private set; }
            public string Message { get; private set; }
        }
    }
}
