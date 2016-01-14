using System;

namespace SuperGlue.EventStore.ConflictManagement
{
    public class ConflictingEventException : Exception
    {
        public ConflictingEventException(string streamName, int oldVersion, int newVersion)
            : base($"The stream: {streamName} has a unresolvable conflicts between version {oldVersion} and {newVersion}")
        {

        }
    }
}