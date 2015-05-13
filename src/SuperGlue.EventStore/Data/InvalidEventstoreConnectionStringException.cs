using System;

namespace SuperGlue.EventStore.Data
{
    public class InvalidEventstoreConnectionStringException : Exception
    {
        public InvalidEventstoreConnectionStringException(string message)
            : base(message)
        {

        }
    }
}