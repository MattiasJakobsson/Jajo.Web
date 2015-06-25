using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IManageProcessManagerStreamEventNumbers
    {
        Task<int?> GetLastEvent(string processManager);
        Task UpdateLastEvent(string processManager, int lastEvent);
    }
}