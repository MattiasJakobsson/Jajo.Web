using EventStore.ClientAPI;

namespace SuperGlue.EventStore
{
    public interface IWriteToErrorStream
    {
        void Write(object evnt, IEventStoreConnection eventStoreConnection, string errorStreamName);
    }
}