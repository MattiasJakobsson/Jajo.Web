namespace SuperGlue.EventTracking
{
    public interface ITrackEntitiesWithEvents
    {
        void Track(ICanApplyEvents canApplyEvents);
        int Count { get; }
        TrackedEntity Pop();
    }
}