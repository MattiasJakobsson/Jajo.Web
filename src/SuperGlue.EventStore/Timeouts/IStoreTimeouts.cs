using System;

namespace SuperGlue.EventStore.Timeouts
{
    public interface IStoreTimeouts
    {
        void GetNextChunk(DateTime startSlice, Action<Tuple<TimeoutData, DateTime>> timeoutFound, out DateTime nextTimeToRunQuery);
        void Add(TimeoutData timeout);
    }
}