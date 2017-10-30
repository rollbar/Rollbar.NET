namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ILogger
    {
        ILogger Log(ErrorLevel level, object obj);
        ILogger Log(ErrorLevel level, string msg);

        ILogger LogCritical(string msg);
        ILogger LogError(string msg);
        ILogger LogWarning(string msg);
        ILogger LogInfo(string msg);
        ILogger LogDebug(string msg);

        ILogger LogCritical(Exception error);
        ILogger LogError(Exception error);

        ILogger LogDebug(ITraceable traceableObj);
        ILogger LogDebug(object obj);
    }
}
