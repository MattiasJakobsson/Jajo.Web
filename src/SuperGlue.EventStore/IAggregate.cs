using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IAggregate
    {
        string Id { get; set; }
        int Version { get; }

        void BuildFromHistory(IEventStream eventStream);
        IEventStream GetUncommittedChanges();
        void ClearUncommittedChanges();
        IDictionary<string, object> GetMetaData(IDictionary<string, object> environment);
        string GetStreamName(IDictionary<string, object> environment);

        event Action<IAggregate> AggregateAttached;
    }
}