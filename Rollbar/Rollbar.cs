using System;
using System.Collections.Generic;

namespace RollbarDotNet
{
    public static class Rollbar
    {
        private static RollbarConfig _config;
        private static Func<Person> _personFunc;

        public static void Init(RollbarConfig config = null)
        {
            if (config == null)
            {
                config = new RollbarConfig();
            }

            _config = config;
        }
        
        public static void PersonData(Func<Person> personFunc)
        {
            _personFunc = personFunc;
        }

        public static Guid? Report(System.Exception e, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(e), level, custom);
        }

        public static Guid? Report(string message, ErrorLevel? level = ErrorLevel.Error, IDictionary<string, object> custom = null)
        {
            return SendBody(new Body(new Message(message)), level, custom);
        }

        private static Guid? SendBody(Body body, ErrorLevel? level, IDictionary<string, object> custom)
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
            payload.Data.Person = _personFunc?.Invoke();

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
