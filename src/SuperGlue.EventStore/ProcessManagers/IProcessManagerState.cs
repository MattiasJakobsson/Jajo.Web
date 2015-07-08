using System.Collections.Generic;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IProcessManagerState
    {
        string Id { get; }
        int Version { get; }

        void BuildFromHistory(IEventStream eventStream);
        IEventStream GetUncommittedChanges();
        void ClearUncommittedChanges();
        IDictionary<string, object> GetMetaData(IDictionary<string, object> environment);

        void TransferState(object evnt);
    }
}