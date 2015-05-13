using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Messages
{
    public class ServiceEventProcessingFailed
    {
        public ServiceEventProcessingFailed(string service, string originalStream, Exception exception, object message, IDictionary<string, object> metaData)
        {
            MetaData = metaData;
            Message = message;
            OriginalStream = originalStream;
            Exception = exception;
            Service = service;
        }

        public string Service { get; private set; }
        public string OriginalStream { get; private set; }
        public Exception Exception { get; private set; }
        public object Message { get; private set; }
        public IDictionary<string, object> MetaData { get; private set; }
    }
}