namespace Rollbar
{
    using System;
    using System.Collections.Generic;

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
        ILogger Warning(Exception error, IDictionary<string, object> custom = null);
        ILogger Info(Exception error, IDictionary<string, object> custom = null);
        ILogger Debug(Exception error, IDictionary<string, object> custom = null);

        ILogger Critical(ITraceable traceableObj, IDictionary<string, object> custom = null);
        ILogger Error(ITraceable traceableObj, IDictionary<string, object> custom = null);
        ILogger Warning(ITraceable traceableObj, IDictionary<string, object> custom = null);
        ILogger Info(ITraceable traceableObj, IDictionary<string, object> custom = null);
        ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null);

        ILogger Critical(object obj, IDictionary<string, object> custom = null);
        ILogger Error(object obj, IDictionary<string, object> custom = null);
        ILogger Warning(object obj, IDictionary<string, object> custom = null);
        ILogger Info(object obj, IDictionary<string, object> custom = null);
        ILogger Debug(object obj, IDictionary<string, object> custom = null);
    }
}
