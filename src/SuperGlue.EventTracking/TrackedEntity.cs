namespace SuperGlue.EventTracking
{
    public class TrackedEntity
    {
        public TrackedEntity(ICanApplyEvents entity)
        {
            Entity = entity;
        }

        public ICanApplyEvents Entity { get; private set; }
    }
}