using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Timeouts
{
    public interface IManageTimeOuts
    {
        Task RequestTimeOut(string writeTo, Guid id, object message, DateTime time, IDictionary<string, object> metaData);
        void Start();
        void Stop();
    }
}