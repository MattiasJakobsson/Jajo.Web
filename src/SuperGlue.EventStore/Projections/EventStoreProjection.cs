using System.Collections.Generic;

namespace SuperGlue.EventStore.Projections
{
    public abstract class EventStoreProjection : IEventStoreProjection
    {
        public virtual string ProjectionName { get { return string.Join("-", GetType().FullName.Split('.')).ToLower(); } }

        public abstract IEnumerable<string> GetInterestingStreams();

        public abstract IApplyStateChangesTo GetStateApplyer(IDictionary<string, object> environment);
    }
}