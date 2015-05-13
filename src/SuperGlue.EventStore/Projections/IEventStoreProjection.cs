using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public interface IEventStoreProjection
    {
        string ProjectionName { get; }
        IEnumerable<string> GetInterestingStreams();
        void Start();
        void Apply(object evnt, int version, IDictionary<string, object> metaData);
        void Commit();
    }
}