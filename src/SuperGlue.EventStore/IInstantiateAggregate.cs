namespace SuperGlue.EventStore
{
    public interface IInstantiateAggregate
    {
        T Instantiate<T>(string id) where T : IAggregate, new();
    }
}