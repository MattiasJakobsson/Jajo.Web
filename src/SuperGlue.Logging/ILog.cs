using System;

namespace SuperGlue.Logging
{
    public interface ILog
    {
        void Debug(string message, params object[] parameters);
        void Debug(string message, Exception exception);
        void Info(string message, params object[] parameters);
        void Info(string message, Exception exception);
        void Warn(string message, params object[] parameters);
        void Warn(string message, Exception exception);
        void Error(string message, params object[] parameters);
        void Error(string message, Exception exception);
        void Fatal(string message, params object[] parameters);
        void Fatal(string message, Exception exception);
    }
}