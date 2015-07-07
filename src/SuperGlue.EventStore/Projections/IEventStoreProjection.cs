using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public interface IEventStoreProjection
    {
        string ProjectionName { get; }
        IEnumerable<string> GetInterestingStreams();
        IApplyStateChangesTo GetStateApplyer(IDictionary<string, object> environment);
    }
}