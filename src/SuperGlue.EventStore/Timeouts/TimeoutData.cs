using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Timeouts
{
    public class TimeoutData
    {
        public TimeoutData(string writeTo, Guid id, object message, DateTime time, IDictionary<string, object> metaData)
        {
            Id = id;
            WriteTo = writeTo;
            MetaData = metaData;
            Time = time;
            Message = message;
        }

        public string WriteTo { get; private set; }
        public Guid Id { get; private set; }
        public object Message { get; private set; }
        public DateTime Time { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }
    }
}