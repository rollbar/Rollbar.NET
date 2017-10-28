namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ILogger
    {
        void Log(ErrorLevel level, object obj);
        void Log(ErrorLevel level, string msg);

        void LogCritical(string msg);
        void LogError(string msg);
        void LogWarning(string msg);
        void LogInfo(string msg);
        void LogDebug(string msg);

        void LogCritical(Exception error);
        void LogError(Exception error);

        void LogDebug(ITraceable traceableObj);
        void LogDebug(object obj);
    }
}
