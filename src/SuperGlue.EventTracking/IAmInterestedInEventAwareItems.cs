namespace SuperGlue.EventTracking
{
    public interface IAmInterestedInEventAwareItems
    {
        void Track(ICanApplyEvents canApplyEvents);
    }
}