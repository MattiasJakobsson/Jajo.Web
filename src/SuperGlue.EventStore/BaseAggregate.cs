using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.EventStore
{
    public abstract class BaseAggregate : IAggregate
    {
        private readonly ICollection<Event> _uncommittedChanges = new Collection<Event>();
        private readonly IList<IEventHandler> _eventHandlers = new List<IEventHandler>();

        protected BaseAggregate(IEventHandlerMappingStrategy eventHandlerMappingStrategy)
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

        public virtual string GetStreamName(IDictionary<string, object> environment)
        {
            return $"aggregate-{GetType().Name}-{Id}";
        }

        public event Action<IAggregate> AggregateAttached;

        protected virtual void OnAggregateAttached(IAggregate aggregate)
        {
            var handler = AggregateAttached;
            handler?.Invoke(aggregate);
        }

        protected void AttachAggregate(IAggregate aggregate, string id)
        {
            aggregate.Id = id;

            OnAggregateAttached(aggregate);
        }

        protected void ApplyEvent(object evnt)
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