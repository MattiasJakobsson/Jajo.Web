namespace SuperGlue.Web.Validation.InputValidation
{
    public interface IValidateInput<in TInput>
    {
        ValidationResult Validate(TInput input);
    }
}