using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Validation
{
    public interface IValidateRequest
    {
        Task<ValidationResult> Validate(IDictionary<string, object> environment);
    }
}