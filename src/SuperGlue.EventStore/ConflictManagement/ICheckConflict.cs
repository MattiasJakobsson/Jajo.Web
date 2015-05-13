namespace SuperGlue.EventStore.ConflictManagement
{
    public interface ICheckConflict<in TNewEvent, in TStoredEvent>
    {
        bool HasConflicts(TNewEvent newEvent, TStoredEvent storedEvent);
    }
}