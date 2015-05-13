using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Timeouts
{
    public interface IManageTimeOuts
    {
        void RequestTimeOut(string writeTo, Guid id, object message, DateTime time, IDictionary<string, object> metaData);
        void Start();
        void Stop();
    }
}