namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ILogger
    {
        ILogger Log(ErrorLevel level, object obj);
        ILogger Log(ErrorLevel level, string msg);

        ILogger Critical(string msg);
        ILogger Error(string msg);
        ILogger Warning(string msg);
        ILogger Info(string msg);
        ILogger Debug(string msg);

        ILogger Critical(Exception error);
        ILogger Error(Exception error);

        ILogger Debug(ITraceable traceableObj);
        ILogger Debug(object obj);
    }
}
