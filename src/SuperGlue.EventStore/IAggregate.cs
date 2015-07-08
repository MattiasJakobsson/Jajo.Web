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
        string GetStreamName(IDictionary<string, object> environment);

        event Action<IAggregate> AggregateAttached;
    }
}