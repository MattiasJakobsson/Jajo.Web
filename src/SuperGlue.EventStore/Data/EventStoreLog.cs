using System;
using System.Collections.Generic;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.Data
{
    public class EventStoreLog : ILogger
    {
        private readonly IDictionary<string, object> _environment;

        public EventStoreLog(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public void Error(string format, params object[] args)
        {
            _environment.Log(format, LogLevel.Error, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _environment.Log(ex, format, LogLevel.Error, args);
        }

        public void Info(string format, params object[] args)
        {
            _environment.Log(format, LogLevel.Info, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _environment.Log(ex, format, LogLevel.Info, args);
        }

        public void Debug(string format, params object[] args)
        {
            _environment.Log(format, LogLevel.Debug, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _environment.Log(ex, format, LogLevel.Debug, args);
        }
    }
}