using System;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Timeouts
{
    public class StoreTimeoutsInMemory : IStoreTimeouts
    {
        public Task<DateTime> GetNextChunk(DateTime startSlice, Action<Tuple<TimeoutData, DateTime>> timeoutFound)
        {
            throw new NotImplementedException();
        }

        public Task Add(TimeoutData timeout)
        {
            throw new NotImplementedException();
        }
    }
}