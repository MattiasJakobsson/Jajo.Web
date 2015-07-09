using System.Collections.Generic;

namespace SuperGlue.Web.Validation.InputValidation
{
    public interface IAmValidatedBy
    {
        IInputValidator GetValidator(IDictionary<string, object> environment);
    }
}