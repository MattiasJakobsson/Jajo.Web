namespace SuperGlue.EventStore.Projections
{
    public interface IManageEventNumbersForProjections
    {
        int? GetLastEvent(string projection);
        void UpdateLastEvent(string projection, int lastEvent);
    }
}