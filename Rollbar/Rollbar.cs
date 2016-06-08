using System;

namespace RollbarDotNet
{
    public static class Rollbar
    {
        private static RollbarConfig _config;

        public static void Init(RollbarConfig config = null)
        {
            if (config == null)
                config = new RollbarConfig();
            _config = config;
        }

        public static Guid? Report(System.Exception e)
        {
            if (string.IsNullOrWhiteSpace(_config.AccessToken))
                return null;

            var guid = Guid.NewGuid();

            var client = new RollbarClient(_config);
            var payload = new Payload(_config.AccessToken, new Data(_config.Environment, new Body(e)));
            payload.Data.GuidUuid = guid;
            client.PostItem(payload);

            return guid;
        }
    }
}