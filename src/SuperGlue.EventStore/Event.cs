using System;

namespace SuperGlue.EventStore
{
    public class Event
    {
        public Event(Guid id, object instance)
        {
            Id = id;
            Instance = instance;
        }

        public Guid Id { get; }
        public object Instance { get; }
    }
}