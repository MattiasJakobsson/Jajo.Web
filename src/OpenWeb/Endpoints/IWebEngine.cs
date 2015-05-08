using System.Threading.Tasks;

namespace OpenWeb.Endpoints
{
    public interface IWebEngine
    {
        Task ExecuteRequest(IRequest request);
    }
}