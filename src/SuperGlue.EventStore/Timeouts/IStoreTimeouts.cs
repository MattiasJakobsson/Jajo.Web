using System;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Timeouts
{
    public interface IStoreTimeouts
    {
        Task<DateTime> GetNextChunk(DateTime startSlice, Action<Tuple<TimeoutData, DateTime>> timeoutFound);
        Task Add(TimeoutData timeout);
    }
}