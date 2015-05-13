namespace SuperGlue.EventStore
{
    public class InstantiateAggregate : IInstantiateAggregate
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