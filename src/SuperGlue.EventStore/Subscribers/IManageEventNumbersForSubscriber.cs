using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Subscribers
{
    public interface IManageEventNumbersForSubscriber
    {
        Task<int?> GetLastEvent(string stream, IDictionary<string, object> environment);
        Task UpdateLastEvent(string stream, int lastEvent, IDictionary<string, object> environment);
    }
}