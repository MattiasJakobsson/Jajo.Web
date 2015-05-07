using System.Threading.Tasks;

namespace OpenWeb
{
    public interface IWebEngine
    {
        Task ExecuteRequest(IRequest request);
    }
}