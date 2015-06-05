using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public interface IWrapMiddleware<TMiddleware>
    {
        Task Before();
        Task After();
    }
}