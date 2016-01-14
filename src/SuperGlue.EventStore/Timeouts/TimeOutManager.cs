using System;

namespace SuperGlue.EventStore.Timeouts
{
    public static class TimeOutManager
    {
        private static readonly object SyncRoot = new object();

        public static Func<IStoreTimeouts> GetCurrent = () => null;

        public static void Configure(Func<IStoreTimeouts> getTimeOutManager)
        {
            lock (SyncRoot)
            {
                GetCurrent = getTimeOutManager;
            }
        }
    }
}