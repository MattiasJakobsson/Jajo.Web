namespace SuperGlue.EventStore
{
    public interface ICheckConflict<in TNewEvent, in TStoredEvent>
    {
        bool HasConflicts(TNewEvent newEvent, TStoredEvent storedEvent);
    }
}