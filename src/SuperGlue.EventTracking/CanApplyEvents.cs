using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public abstract class CanApplyEvents : ICanApplyEvents
    {
        private const string EntityClrTypeHeader = "EntityClrTypeName";
        private const string CommitIdHeader = "CommitId";
        private const string EntityIdHeader = "EntityId";

        private readonly ICollection<object> _events = new Collection<object>();

        public string Id { get; set; }

        public IEnumerable<object> GetAppliedEvents()
        {
            return new ReadOnlyCollection<object>(_events.ToList());
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
                {CommitIdHeader, Guid.NewGuid()},
                {EntityIdHeader, Id}
            };
        }

        public string GetStreamName(IDictionary<string, object> environment)
        {
            return string.Format("entity-{0}-{1}", GetType().Name, Id.Replace("/", "-"));
        }

        protected void ApplyEvent(object evnt)
        {
            _events.Add(evnt);
        }
    }
}