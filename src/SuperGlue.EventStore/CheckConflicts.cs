using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using SuperGlue.Web;

namespace SuperGlue.EventStore
{
    public class CheckConflicts : ICheckConflicts
    {
        private readonly IHandleEventSerialization _eventSerialization;

        public CheckConflicts(IHandleEventSerialization eventSerialization)
        {
            _eventSerialization = eventSerialization;
        }

        public bool HasConflicts(IEnumerable<object> newEvents, IEnumerable<ResolvedEvent> storedEvents, IDictionary<string, object> environment)
        {
            var storedEventsList = storedEvents.Select(x => _eventSerialization.DeSerialize(x.Event.EventId, x.Event.EventNumber, x.OriginalEventNumber, x.Event.Metadata, x.Event.Data)).ToList();

            foreach (var newEvent in newEvents)
            {
                foreach (var storedEvent in storedEventsList)
                {
                    var conflictCheckers = environment.ResolveAll(typeof(ICheckConflict<,>).MakeGenericType(newEvent.GetType(), storedEvent.Data.GetType())).OfType<object>().ToList();

                    foreach (var conflictChecker in conflictCheckers)
                    {
                        if (((bool)conflictChecker.GetType()
                            .GetMethod("HasConflicts", new[] { newEvent.GetType(), storedEvent.GetType() })
                            .Invoke(conflictChecker, new[] { newEvent, storedEvent.Data })))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}