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

        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(level, obj.ToString(), custom);
        }

        public ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null)
        {
            this.Report(msg, level, custom);

            return this;
        }

        public ILogger Critical(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Critical, msg, custom);
        }

        public ILogger Critical(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Critical, custom);

            return this;
        }

        public ILogger Debug(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Debug, msg, custom);
        }

        public ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null)
        {
            return this.Debug(traceableObj.Trace(), custom);
        }

        public ILogger Debug(object obj, IDictionary<string, object> custom = null)
        {
            return this.Debug(obj.ToString(), custom);
        }

        public ILogger Error(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Error, msg, custom);
        }

        public ILogger Error(System.Exception error, IDictionary<string, object> custom = null)
        {
            this.Report(error, ErrorLevel.Error, custom);

            return this;
        }

        public ILogger Info(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Info, msg, custom);
        }

        public ILogger Warning(string msg, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Warning, msg, custom);
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
