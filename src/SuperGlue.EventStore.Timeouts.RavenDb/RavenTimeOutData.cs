using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore.Timeouts.RavenDb
{
    public class RavenTimeOutData
    {
        public string Id { get; set; }
        public string OwningTimeOutManager { get; set; }
        public DateTime Time { get; set; }
        public string WriteTo { get; set; }
        public Guid CommitId { get; set; }
        public object Message { get; set; }
        public IDictionary<string, object> MetaData { get; set; }

        public TimeoutData GetTimeoutData()
        {
            return new TimeoutData(WriteTo, CommitId, Message, Time, MetaData);
        }

        public static string BuildId(Guid id)
        {
            return string.Format("TimeOuts/{0}", id);
        }
    }
}