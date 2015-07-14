using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IManageProcessManagerStreamEventNumbers
    {
        Task<int?> GetLastEvent(string processManager, IDictionary<string, object> environment);
        Task UpdateLastEvent(string processManager, int lastEvent, IDictionary<string, object> environment);
    }
}