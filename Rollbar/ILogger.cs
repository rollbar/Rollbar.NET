namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ILogger
    {
        ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null);
        ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null);

        ILogger Critical(string msg, IDictionary<string, object> custom = null);
        ILogger Error(string msg, IDictionary<string, object> custom = null);
        ILogger Warning(string msg, IDictionary<string, object> custom = null);
        ILogger Info(string msg, IDictionary<string, object> custom = null);
        ILogger Debug(string msg, IDictionary<string, object> custom = null);

        ILogger Critical(Exception error, IDictionary<string, object> custom = null);
        ILogger Error(Exception error, IDictionary<string, object> custom = null);

        ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null);
        ILogger Debug(object obj, IDictionary<string, object> custom = null);
    }
}
