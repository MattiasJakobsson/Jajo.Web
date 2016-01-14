using System;

namespace SuperGlue.EventTracking
{
    public class TrackedEvent
    {
        public TrackedEvent(Guid id, object instance)
        {
            Id = id;
            Instance = instance;
        }

        public Guid Id { get; }
        public object Instance { get; } 
    }
}