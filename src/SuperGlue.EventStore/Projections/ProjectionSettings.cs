using System;

namespace SuperGlue.EventStore.Projections
{
    public class ProjectionSettings
    {
        private int _dispatchBufferSeconds = 1;
        private int _dispatchBufferNumber = 256;

        public ProjectionSettings BufferSeconds(int seconds)
        {
            if(seconds < 0)
                throw new ArgumentException("seconds can't be negative", "seconds");

            _dispatchBufferSeconds = seconds;

            return this;
        }

        public ProjectionSettings BufferNumber(int number)
        {
            if (number < 0)
                throw new ArgumentException("number can't be negative", "number");

            _dispatchBufferNumber = number;

            return this;
        }

        public BufferSettings GetBufferSettings()
        {
            return new BufferSettings(_dispatchBufferSeconds, _dispatchBufferNumber);
        }

        public class BufferSettings
        {
            public BufferSettings(int seconds, int numberOfEvents)
            {
                Seconds = seconds;
                NumberOfEvents = numberOfEvents;
            }

            public int Seconds { get; private set; }
            public int NumberOfEvents { get; private set; } 
        }
    }
}