using System;

namespace SuperGlue.EventStore
{
    public class ConflictingEventException : Exception
    {
        public ConflictingEventException(string streamName, int oldVersion, int newVersion)
            : base(string.Format("The stream: {0} has a unresolvable conflicts between version {1} and {2}", streamName, oldVersion, newVersion))
        {

        }
    }
}