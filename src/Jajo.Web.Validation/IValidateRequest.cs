using System.Collections.Generic;

namespace Jajo.Web.Validation
{
    public interface IValidateRequest
    {
        ValidationResult Validate(IDictionary<string, object> environment);
    }
}