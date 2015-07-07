using System;
using NLog;

namespace SuperGlue.Logging.NLog
{
    public class NLogLogger : ILog
    {
        private readonly Logger _logger;

        public NLogLogger(Logger logger)
        {
            _logger = logger;
        }

        public void Debug(string message, params object[] parameters)
        {
            _logger.Debug(message, parameters);
        }

        public void Debug(string message, Exception exception)
        {
            _logger.Debug(exception, message);
        }

        public void Info(string message, params object[] parameters)
        {
            _logger.Info(message, parameters);
        }

        public void Info(string message, Exception exception)
        {
            _logger.Info(exception, message);
        }

        public void Warn(string message, params object[] parameters)
        {
            _logger.Warn(message, parameters);
        }

        public void Warn(string message, Exception exception)
        {
            _logger.Warn(exception, message);
        }

        public void Error(string message, params object[] parameters)
        {
            _logger.Error(message, parameters);
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(exception, message);
        }

        public void Fatal(string message, params object[] parameters)
        {
            _logger.Fatal(message, parameters);
        }

        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal(exception, message);
        }
    }
}