namespace SuperGlue.EventStore
{
    public class DefaultAggregateInstantiator : IInstantiateAggregate
    {
        public T Instantiate<T>(string id) where T : IAggregate, new()
        {
            return new T
            {
                Id = id
            };
        }
    }
}