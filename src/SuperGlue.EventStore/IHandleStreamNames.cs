namespace SuperGlue.EventStore
{
    public interface IHandleStreamNames
    {
        string GetAggregateStreamName(IAggregate aggregate);
        string GetStreamName(string stream, string context);
    }
}