using System;

namespace SuperGlue.EventStore
{
    public class TypeThresholdedActionBasedDomainEventHandler : IEventHandler
    {
        private readonly Type _eventTypeThreshold;
        private readonly bool _exact;
        private readonly Action<object> _handler;

        public TypeThresholdedActionBasedDomainEventHandler(Action<object> handler, Type eventTypeThreshold, bool exact = false)
        {
            _handler = handler;
            _eventTypeThreshold = eventTypeThreshold;
            _exact = exact;
        }

        public bool Handle(object evnt)
        {
            var handled = false;

            if (ShouldHandleThisEventData(evnt))
            {
                _handler(evnt);
                handled = true;
            }

            return handled;
        }

        private bool ShouldHandleThisEventData(object evnt)
        {
            var shouldHandle = false;

            var dataType = evnt.GetType();

            if (_eventTypeThreshold.IsAssignableFrom(dataType))
            {
                if (_exact)
                {
                    shouldHandle = (_eventTypeThreshold == dataType);
                }
                else
                {
                    shouldHandle = true;
                }
            }

            return shouldHandle;
        }
    }
}