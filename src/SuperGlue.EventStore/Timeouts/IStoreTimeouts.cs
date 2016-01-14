using System;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Timeouts
{
    public interface IStoreTimeouts
    {
        Task<DateTime> GetNextChunk(DateTime startSlice, Func<Tuple<TimeoutData, DateTime>, Task> timeoutFound);
        Task Add(TimeoutData timeout);
    }
}