using System;

namespace Rollbar
{
    public class RollbarLogger
        : IRollbar
    {
        public ILogger Logger => throw new NotImplementedException();

        public IRollbar Configure(RollbarConfig settings)
        {
            throw new NotImplementedException();
        }

        public IRollbar Configure(string accessToken)
        {
            throw new NotImplementedException();
        }

        public RollbarConfig CloneConfiguration()
        {
            throw new NotImplementedException();
        }

        public void Log(ErrorLevel level, object obj)
        {
            throw new NotImplementedException();
        }

        public void Log(ErrorLevel level, string msg)
        {
            throw new NotImplementedException();
        }

        public void LogCritical(string msg)
        {
            throw new NotImplementedException();
        }

        public void LogCritical(Exception error)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(string msg)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(ITraceable traceableObj)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(object obj)
        {
            throw new NotImplementedException();
        }

        public void LogError(string msg)
        {
            throw new NotImplementedException();
        }

        public void LogError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(string msg)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string msg)
        {
            throw new NotImplementedException();
        }
    }
}
