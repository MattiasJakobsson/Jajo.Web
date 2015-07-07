using System;
using System.Collections.Generic;
using SuperGlue.EventStore.ProcessManagers;
using SuperGlue.EventStore.Projections;

namespace SuperGlue.EventStore
{
    public static class EventStoreEnvironmentExtensions
    {
        public static class EventStoreConstants
        {
            public const string ProcessManager = "superglue.EventStore.ProcessManager";
            public const string Projection = "superglue.EventStore.Projection";
            public const string Service = "superglue.EventStore.Service";
            public const string Stream = "superglue.EventStore.Stream";
            public const string IsCatchUp = "superglue.EventStore.IsCatchUp";
            public const string Events = "superglue.EventStore.Events";
            public const string OnException = "superglue.EventStore.OnException";
            public const string EventsGroupedBy = "superglue.EventStore.EventsGroupedBy";
        }

        public static EventStoreRequestEnvironment GetEventStoreRequest(this IDictionary<string, object> environment)
        {
            return new EventStoreRequestEnvironment(environment);
        }

        public static object GetEventsGroupedBy(this IDictionary<string, object> environment)
        {
            return environment.Get<object>(EventStoreConstants.EventsGroupedBy);
        }

        public class EventStoreRequestEnvironment
        {
            private readonly IDictionary<string, object> _environment;

            public EventStoreRequestEnvironment(IDictionary<string, object> environment)
            {
                _environment = environment;
            }

            public IManageProcess ProcessManager
            {
                get { return _environment.Get<IManageProcess>(EventStoreConstants.ProcessManager); }
                set { _environment[EventStoreConstants.ProcessManager] = value; }
            }

            public IEventStoreProjection Projection
            {
                get { return _environment.Get<IEventStoreProjection>(EventStoreConstants.Projection); }
                set { _environment[EventStoreConstants.Projection] = value; }
            }

            public IEnumerable<DeSerializationResult> Events
            {
                get { return _environment.Get<IEnumerable<DeSerializationResult>>(EventStoreConstants.Events) ?? new List<DeSerializationResult>(); }
                set { _environment[EventStoreConstants.Events] = value; }
            }

            public string Service
            {
                get { return _environment.Get<string>(EventStoreConstants.Service); }
                set { _environment[EventStoreConstants.Service] = value; }
            }

            public string Stream
            {
                get { return _environment.Get<string>(EventStoreConstants.Stream); }
                set { _environment[EventStoreConstants.Stream] = value; }
            }

            public bool IsCatchUp
            {
                get { return _environment.Get<bool>(EventStoreConstants.IsCatchUp); }
                set { _environment[EventStoreConstants.IsCatchUp] = value; }
            }

            public Action<Exception, DeSerializationResult> OnException
            {
                get { return _environment.Get<Action<Exception, DeSerializationResult>>(EventStoreConstants.OnException) ?? ((x, y) => { }); }
                set { _environment[EventStoreConstants.OnException] = value; }
            }
        }
    }
}