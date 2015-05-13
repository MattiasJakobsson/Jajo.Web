namespace SuperGlue.EventStore.Subscribers
{
    public interface IManageEventNumbersForSubscriber
    {
        int? GetLastEvent(string service, string stream);
        void UpdateLastEvent(string service, string stream, int lastEvent);
    }
}