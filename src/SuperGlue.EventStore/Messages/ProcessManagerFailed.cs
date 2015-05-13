using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Messages
{
    public class ProcessManagerFailed
    {
        public ProcessManagerFailed(string processName, Exception exception, object message, IDictionary<string, object> metaData)
        {
            MetaData = metaData;
            Message = message;
            Exception = exception;
            ProcessName = processName;
        }

        public string ProcessName { get; private set; }
        public Exception Exception { get; private set; }
        public object Message { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }
    }
}