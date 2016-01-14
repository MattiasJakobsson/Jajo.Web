using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public abstract class CanApplyEvents : ICanApplyEvents
    {
        private const string EntityClrTypeHeader = "EntityClrTypeName";
        private const string EntityIdHeader = "EntityId";

        private readonly ICollection<TrackedEvent> _events = new Collection<TrackedEvent>();

        public string Id { get; set; }

        public IEnumerable<TrackedEvent> GetAppliedEvents()
        {
            return new ReadOnlyCollection<TrackedEvent>(_events.ToList());
        }

        public void ClearAppliedEvents()
        {
            _events.Clear();
        }

        public IDictionary<string, object> GetMetaData(IDictionary<string, object> environment)
        {
            return new Dictionary<string, object>
            {
                {EntityClrTypeHeader, GetType().AssemblyQualifiedName},
                {EntityIdHeader, Id}
            };
        }

        public string GetStreamName(IDictionary<string, object> environment)
        {
            return $"entity-{GetType().Name}-{Id.Replace("/", "-")}";
        }

        protected void ApplyEvent(object evnt)
        {
            _events.Add(new TrackedEvent(Guid.NewGuid(), evnt));
        }
    }
}