using System.Threading.Tasks;

namespace SuperGlue.Web.Validation.InputValidation
{
    public interface IInputValidator
    {
        Task<ValidationResult> Validate();
    }
}