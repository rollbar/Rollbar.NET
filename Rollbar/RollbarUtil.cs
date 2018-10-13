namespace Rollbar
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class RollbarUtil
    {
        public static ILogger LogUsingProperObjectDiscovery(IRollbar rollbar, ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            if (rollbar.Config.LogLevel.HasValue && level < rollbar.Config.LogLevel.Value)
            {
                // nice shortcut:
                return rollbar;
            }

            Data data = obj as Data;
            if (data != null)
            {
                data.Level = level;
                return rollbar.Log(data);
            }
            System.Exception exception = obj as System.Exception;
            if (exception != null)
            {
                Body exceptionBody = new Body(exception);
                Data exceptionData = new Data(rollbar.Config, exceptionBody, custom);
                rollbar.Log(exceptionData);
                return rollbar;
            }
            ITraceable traceable = obj as ITraceable;
            if (traceable != null)
            {
                return rollbar.Log(level, traceable.TraceAsString(), custom);
            }

            return rollbar.Log(level, obj.ToString(), custom);
        }
    }
}
