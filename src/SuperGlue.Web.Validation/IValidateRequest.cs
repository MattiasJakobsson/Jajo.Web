using System.Collections.Generic;

namespace SuperGlue.Web.Validation
{
    public interface IValidateRequest
    {
        ValidationResult Validate(IDictionary<string, object> environment);
    }
}