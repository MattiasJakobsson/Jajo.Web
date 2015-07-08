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

        public void Debug(Exception exception, string message, params object[] parameters)
        {
            _logger.Debug(exception, message, parameters);
        }

        public void Info(string message, params object[] parameters)
        {
            _logger.Info(message, parameters);
        }

        public void Info(Exception exception, string message, params object[] parameters)
        {
            _logger.Info(exception, message, parameters);
        }

        public void Warn(string message, params object[] parameters)
        {
            _logger.Warn(message, parameters);
        }

        public void Warn(Exception exception, string message, params object[] parameters)
        {
            _logger.Warn(exception, message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            _logger.Error(message, parameters);
        }

        public void Error(Exception exception, string message, params object[] parameters)
        {
            _logger.Error(exception, message, parameters);
        }

        public void Fatal(string message, params object[] parameters)
        {
            _logger.Fatal(message, parameters);
        }

        public void Fatal(Exception exception, string message, params object[] parameters)
        {
            _logger.Fatal(exception, message, parameters);
        }
    }
}