using System.Threading.Tasks;

namespace SuperGlue.EventStore.Subscribers
{
    public interface IManageEventNumbersForSubscriber
    {
        Task<int?> GetLastEvent(string service, string stream);
        Task UpdateLastEvent(string service, string stream, int lastEvent);
    }
}