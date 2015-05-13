namespace SuperGlue.EventStore.Projections
{
    public interface IProjectionState
    {
        void SetVersion(int version);
    }
}