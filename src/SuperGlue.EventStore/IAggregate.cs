using System;

namespace SuperGlue.EventStore
{
    public interface IAggregate
    {
        string Id { get; set; }
        int Version { get; }
        string Context { get; }

        void BuildFromHistory(IEventStream eventStream);
        IEventStream GetUncommittedChanges();
        void ClearUncommittedChanges();

        event Action<IAggregate> AggregateAttached;
    }
}