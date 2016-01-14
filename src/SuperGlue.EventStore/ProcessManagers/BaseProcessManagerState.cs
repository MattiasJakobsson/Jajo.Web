using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventStore.ProcessManagers
{
    public abstract class BaseProcessManagerState : IProcessManagerState
    {
        private const string AggregateClrTypeHeader = "AggregateClrTypeName";
        private const string CommitIdHeader = "CommitId";
        private const string AggregateIdHeader = "AggregateId";

        private readonly ICollection<Event> _uncommittedChanges = new Collection<Event>();
        private readonly IList<IEventHandler> _eventHandlers = new List<IEventHandler>();

        protected BaseProcessManagerState(IEventHandlerMappingStrategy eventHandlerMappingStrategy)
        {
            Version = 0;
            InitializeEventHandlers(eventHandlerMappingStrategy);
        }

        public string Id { get; set; }
        public int Version { get; private set; }

        public void BuildFromHistory(IEventStream eventStream)
        {
            if (_uncommittedChanges.Count > 0)
                throw new InvalidOperationException("Cannot apply history when instance has uncommitted changes.");

            foreach (var evnt in eventStream.Events)
                HandleEvent(evnt);
        }

        public IEventStream GetUncommittedChanges()
        {
            var changes = new List<Event>(_uncommittedChanges);
            return new EventStream(changes);
        }

        public void ClearUncommittedChanges()
        {
            _uncommittedChanges.Clear();
        }

        public IDictionary<string, object> GetMetaData(IDictionary<string, object> environment)
        {
            return new Dictionary<string, object>
            {
                {AggregateClrTypeHeader, GetType().AssemblyQualifiedName},
                {CommitIdHeader, Guid.NewGuid()},
                {AggregateIdHeader, Id}
            };
        }

        public void TransferState(object evnt)
        {
            _uncommittedChanges.Add(new Event(Guid.NewGuid(), evnt));
            HandleEvent(evnt);
        }

        private void HandleEvent(object evnt)
        {
            var handlers = new List<IEventHandler>(_eventHandlers);

            handlers.Aggregate(false, (current, handler) => current | handler.Handle(evnt));

            Version++;
        }

        private void AddEventHandler(IEventHandler eventHandler)
        {
            _eventHandlers.Add(eventHandler);
        }

        private void InitializeEventHandlers(IEventHandlerMappingStrategy eventHandlerMappingStrategy)
        {
            foreach (var eventHandler in eventHandlerMappingStrategy.GetEventHandlers(this))
                AddEventHandler(eventHandler);
        }
    }
}