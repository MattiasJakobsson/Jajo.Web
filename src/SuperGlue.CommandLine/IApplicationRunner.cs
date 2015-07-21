using System.Threading.Tasks;

namespace SuperGlue
{
    public interface IApplicationRunner
    {
        Task Start(string environment);
        Task Stop();
        Task Recycle(string environment);
    }
}