using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Projections
{
    public interface IManageEventNumbersForProjections
    {
        Task<int?> GetLastEvent(string projection, IDictionary<string, object> environment);
        Task UpdateLastEvent(string projection, int lastEvent, IDictionary<string, object> environment);
    }
}