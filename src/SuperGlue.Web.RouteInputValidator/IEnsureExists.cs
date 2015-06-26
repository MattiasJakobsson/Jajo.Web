using System.Threading.Tasks;

namespace SuperGlue.Web.RouteInputValidator
{
    public interface IEnsureExists<in TInput>
    {
        Task<bool> Exists(TInput input);
    }
}