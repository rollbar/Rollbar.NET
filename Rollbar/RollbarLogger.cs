using Rollbar.DTOs;
using System;
using System.Collections.Generic;

namespace Rollbar
{
    public class RollbarLogger
        : IRollbar
    {
        private RollbarConfig _config;

        #region IRollbar

        public ILogger Logger => this;

        public IRollbar Configure(RollbarConfig settings)
        {
            this._config = settings;

            return this;
        }

        public IRollbar Configure(string accessToken)
        {
            this._config = new RollbarConfig(accessToken);

            return this;
        }

        public RollbarConfig CloneConfiguration()
        {
            return this._config;
        }

        #endregion IRollbar

        #region ILogger

        public ILogger Log(ErrorLevel level, object obj)
        {
            return this.Log(level, obj.ToString());
        }

        public ILogger Log(ErrorLevel level, string msg)
        {
            this.Report(msg, level);

            return this;
        }

        public ILogger LogCritical(string msg)
        {
            return this.Log(ErrorLevel.Critical, msg);
        }

        public ILogger LogCritical(System.Exception error)
        {
            this.Report(error, ErrorLevel.Critical);

            return this;
        }

        public ILogger LogDebug(string msg)
        {
            return this.Log(ErrorLevel.Debug, msg);
        }

        public ILogger LogDebug(ITraceable traceableObj)
        {
            return this.LogDebug(traceableObj.Trace());
        }

        public ILogger LogDebug(object obj)
        {
            return this.LogDebug(obj.ToString());
        }

        public ILogger LogError(string msg)
        {
            return this.Log(ErrorLevel.Error, msg);
        }

        public ILogger LogError(System.Exception error)
        {
            this.Report(error, ErrorLevel.Error);

            return this;
        }

        public ILogger LogInfo(string msg)
        {
            return this.Log(ErrorLevel.Info, msg);
        }

        public ILogger LogWarning(string msg)
        {
            return this.Log(ErrorLevel.Warning, msg);
        }

        #endregion ILogger

        private Guid? Report(System.Exception e, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(e), level, custom);
        }

        private Guid? Report(string message, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(new Message(message)), level, custom);
        }

        private Guid? SendBody(Body body, ErrorLevel? level, IDictionary<string, object> custom)
        {
            if (string.IsNullOrWhiteSpace(_config.AccessToken) || _config.Enabled == false)
            {
                return null;
            }

            var guid = Guid.NewGuid();

            var client = new RollbarClient(_config);
            var data = new Data(_config.Environment, body)
            {
                Custom = custom,
                Level = level ?? _config.LogLevel
            };

            var payload = new Payload(_config.AccessToken, data);
            payload.Data.GuidUuid = guid;
            payload.Data.Person = _config.Person;

            if (_config.Server != null)
            {
                payload.Data.Server = _config.Server;
            }

            _config.Transform?.Invoke(payload);
            client.PostItem(payload);

            return guid;
        }

    }
}
