using System.Collections.Generic;
using System.Linq;
using EventStore.ClientAPI;
using SuperGlue.Configuration;

namespace SuperGlue.EventStore.ConflictManagement
{
    public class DefaultConflictChecker : ICheckConflicts
    {
        private readonly IHandleEventSerialization _eventSerialization;

        public DefaultConflictChecker(IHandleEventSerialization eventSerialization)
        {
            _eventSerialization = eventSerialization;
        }

        public bool HasConflicts(IEnumerable<object> newEvents, IEnumerable<ResolvedEvent> storedEvents, IDictionary<string, object> environment)
        {
            var storedEventsList = storedEvents.Select(x => _eventSerialization.DeSerialize(x)).ToList();

            //TODO:Refactor

            foreach (var newEvent in newEvents)
            {
                foreach (var storedEvent in storedEventsList)
                {
                    var conflictCheckers = environment.ResolveAll(typeof(ICheckConflict<,>).MakeGenericType(newEvent.GetType(), storedEvent.Data.GetType())).ToList();

                    foreach (var conflictChecker in conflictCheckers)
                    {
                        if ((bool)conflictChecker.GetType()
                            .GetMethod("HasConflicts", new[] { newEvent.GetType(), storedEvent.GetType() })
                            .Invoke(conflictChecker, new[] { newEvent, storedEvent.Data }))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}