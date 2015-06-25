using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public class DefaultEventEntitiesTracker : ITrackEntitiesWithEvents
    {
        private readonly object _lockObject = new object();
        private readonly Stack<TrackedEntity> _canApplyEvents = new Stack<TrackedEntity>();

        public void Track(ICanApplyEvents canApplyEvents, object associatedCommand, IDictionary<string, object> commandMetaData)
        {
            lock (_lockObject)
            {
                var current = _canApplyEvents.FirstOrDefault(x => x.Entity == canApplyEvents);

                if (current != null)
                {
                    current.SetMetaData(associatedCommand, commandMetaData);
                    return;
                }

                _canApplyEvents.Push(new TrackedEntity(canApplyEvents, associatedCommand, commandMetaData));
            }
        }

        public int Count { get { return _canApplyEvents.Count; } }

        public TrackedEntity Pop()
        {
            lock (_lockObject)
            {
                return _canApplyEvents.Pop();
            }
        }
    }
}