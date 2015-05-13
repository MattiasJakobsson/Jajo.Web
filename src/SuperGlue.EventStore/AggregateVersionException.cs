using System;

namespace SuperGlue.EventStore
{
    public class AggregateVersionException : Exception
    {
        public string Id { get; private set; }
        public Type Type { get; private set; }
        public int AggregateVersion { get; private set; }
        public int RequestedVersion { get; private set; }

        public AggregateVersionException(string id, Type type, int aggregateVersion, int requestedVersion)
            : base(string.Format("Requested version {2} of aggregate '{0}' (type {1}) - aggregate version is {3}", id, type.Name, requestedVersion, aggregateVersion))
        {
            Id = id;
            Type = type;
            AggregateVersion = aggregateVersion;
            RequestedVersion = requestedVersion;
        }
    }
}