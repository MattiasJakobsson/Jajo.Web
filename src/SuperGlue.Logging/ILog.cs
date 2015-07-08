using System;

namespace SuperGlue.Logging
{
    public interface ILog
    {
        void Debug(string message, params object[] parameters);
        void Debug(Exception exception, string message, params object[] parameters);
        void Info(string message, params object[] parameters);
        void Info(Exception exception, string message, params object[] parameters);
        void Warn(string message, params object[] parameters);
        void Warn(Exception exception, string message, params object[] parameters);
        void Error(string message, params object[] parameters);
        void Error(Exception exception, string message, params object[] parameters);
        void Fatal(string message, params object[] parameters);
        void Fatal(Exception exception, string message, params object[] parameters);
    }
}