using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventTracking
{
    public abstract class CanApplyEvents : ICanApplyEvents
    {
        private readonly ICollection<object> _events = new Collection<object>();

        public string Id { get; set; }
        public abstract string Context { get; }

        public IEnumerable<object> GetAppliedEvents()
        {
            return new ReadOnlyCollection<object>(_events.ToList());
        }

        public void ClearAppliedEvents()
        {
            _events.Clear();
        }

        protected void ApplyEvent(object evnt)
        {
            _events.Add(evnt);
        }
    }
}