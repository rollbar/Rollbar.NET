namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAsyncLogger
    {
        Task Log(DTOs.Data rollbarData); 
        Task Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null);
        Task Critical(object obj, IDictionary<string, object> custom = null);
        Task Error(object obj, IDictionary<string, object> custom = null);
        Task Warning(object obj, IDictionary<string, object> custom = null);
        Task Info(object obj, IDictionary<string, object> custom = null);
        Task Debug(object obj, IDictionary<string, object> custom = null);

        ILogger AsBlockingLogger(TimeSpan timeout);
    }
}
