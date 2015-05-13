using System;

namespace SuperGlue.EventStore
{
    public class InvalidEventstoreConnectionStringException : Exception
    {
        public InvalidEventstoreConnectionStringException(string message)
            : base(message)
        {

        }
    }
}