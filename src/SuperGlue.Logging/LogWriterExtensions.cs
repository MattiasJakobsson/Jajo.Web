using System;
using System.Collections.Generic;

namespace SuperGlue.Logging
{
    public static class LogWriterExtensions
    {
        private static readonly IReadOnlyDictionary<string, Action<ILog, Exception, string, object[]>> LogLevelLoggers = new Dictionary<string, Action<ILog, Exception, string, object[]>>
        {
            {LogLevel.Debug, Debug},
            {LogLevel.Info, Info},
            {LogLevel.Warn, Warn},
            {LogLevel.Error, Error},
            {LogLevel.Fatal, Fatal}
        };

        public static void Log(this ILog log, Exception exception, string message, string logLevel, object[] parameters)
        {
            if(!LogLevelLoggers.ContainsKey(logLevel)) 
                return;
            
            LogLevelLoggers[logLevel](log, exception, message, parameters);
        }

        public static void Log(this ILog log, string message, string logLevel, object[] parameters)
        {
            Log(log, null, message, logLevel, parameters);
        }

        private static void Debug(ILog log, Exception exception, string message, object[] parameters)
        {
            log.Debug(exception, message, parameters);
        }

        private static void Info(ILog log, Exception exception, string message, object[] parameters)
        {
            log.Info(exception, message, parameters);
        }

        private static void Warn(ILog log, Exception exception, string message, object[] parameters)
        {
            log.Warn(exception, message, parameters);
        }

        private static void Error(ILog log, Exception exception, string message, object[] parameters)
        {
            log.Error(exception, message, parameters);
        }

        private static void Fatal(ILog log, Exception exception, string message, object[] parameters)
        {
            log.Fatal(exception, message, parameters);
        }
    }
}