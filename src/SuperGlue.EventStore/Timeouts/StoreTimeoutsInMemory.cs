using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Timeouts
{
    public class StoreTimeoutsInMemory : IStoreTimeouts
    {
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();
        private static readonly List<TimeoutData> Storage = new List<TimeoutData>();

        public async Task<DateTime> GetNextChunk(DateTime startSlice, Func<Tuple<TimeoutData, DateTime>, Task> timeoutFound)
        {
            var now = DateTime.UtcNow;
            var nextTimeToRunQuery = DateTime.MaxValue;
            var dueTimeouts = new List<TimeoutData>();

            try
            {
                ReaderWriterLock.EnterReadLock();

                foreach (var data in Storage)
                {
                    if (data.Time > now && data.Time < nextTimeToRunQuery)
                    {
                        nextTimeToRunQuery = data.Time;
                    }
                    if (data.Time > startSlice && data.Time <= now)
                    {
                        dueTimeouts.Add(data);
                    }
                }
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }

            if (nextTimeToRunQuery == DateTime.MaxValue)
                nextTimeToRunQuery = now.AddMinutes(1);

            foreach (var timeout in dueTimeouts)
                await timeoutFound(new Tuple<TimeoutData, DateTime>(timeout, timeout.Time)).ConfigureAwait(false);

            return nextTimeToRunQuery;
        }

        public Task Add(TimeoutData timeout)
        {
            try
            {
                ReaderWriterLock.EnterWriteLock();
                Storage.Add(timeout);
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }

            return Task.CompletedTask;
        }
    }
}