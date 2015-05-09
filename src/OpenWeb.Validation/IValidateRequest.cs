using System.Collections.Generic;

namespace OpenWeb.Validation
{
    public interface IValidateRequest
    {
        ValidationResult Validate(IDictionary<string, object> environment);
    }
}