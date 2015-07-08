using System;
using EventStore.ClientAPI;
using SuperGlue.Logging;

namespace SuperGlue.EventStore.Data
{
    public class EventStoreLog : ILogger
    {
        private readonly ILog _log;

        public EventStoreLog(ILog log)
        {
            _log = log;
        }

        public void Error(string format, params object[] args)
        {
            _log.Error(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _log.Error(ex, format, args);
        }

        public void Info(string format, params object[] args)
        {
            _log.Info(format, args);
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _log.Info(ex, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            _log.Debug(format, args);
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _log.Debug(ex, format, args);
        }
    }
}