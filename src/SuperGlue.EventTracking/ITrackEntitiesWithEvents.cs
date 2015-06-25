using System.Collections.Generic;

namespace SuperGlue.EventTracking
{
    public interface ITrackEntitiesWithEvents
    {
        void Track(ICanApplyEvents canApplyEvents, object associatedCommand, IDictionary<string, object> commandMetaData);
        int Count { get; }
        TrackedEntity Pop();
    }
}