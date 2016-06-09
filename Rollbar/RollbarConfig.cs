using System;

namespace RollbarDotNet
{
    public class RollbarConfig
    {
        public RollbarConfig() : this("")
        {
        }

        public RollbarConfig(string accessToken)
        {
            AccessToken = accessToken;
            Environment = "production";
            Enabled = true;
            LogLevel = ErrorLevel.Debug;
            ScrubFields = new[] { "passwd", "password", "secret", "confirm_password", "password_confirmation" };
            EndPoint = "https://api.rollbar.com/api/1/";
        }

        public string AccessToken { get; set; }

        public string EndPoint { get; set; }

        public string[] ScrubFields { get; set; }

        public ErrorLevel? LogLevel { get; set; }

        public bool? Enabled { get; set; }

        public string Environment { get; set; }

        public Action<Payload> Transform { get; set; }
    }
}