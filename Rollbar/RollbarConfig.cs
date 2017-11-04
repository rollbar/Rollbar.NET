namespace Rollbar
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public struct RollbarConfig
        : ITraceable
    {
        //public RollbarConfig() : this("")
        //{
        //}

        public RollbarConfig(string accessToken)
        {
            AccessToken = accessToken;

            // let's set some default values:
            Environment = "production";
            Enabled = true;
            LogLevel = ErrorLevel.Debug;
            ScrubFields = new[] { "passwd", "password", "secret", "confirm_password", "password_confirmation" };
            EndPoint = "https://api.rollbar.com/api/1/";
            ProxyAddress = null;
            Transform = null;
            Server = null;
            Person = null;
        }

        public string AccessToken { get; set; }

        public string EndPoint { get; set; }

        public string[] ScrubFields { get; set; }

        public ErrorLevel? LogLevel { get; set; }

        public bool? Enabled { get; set; }

        public string Environment { get; set; }

        public Action<Payload> Transform { get; set; }

        public Server Server { get; set; }

        public Person Person { get;set; }

        public string ProxyAddress { get; set; }

        public string Trace(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  AccessToken: " + this.AccessToken);
            sb.AppendLine(indent + "  EndPoint: " + this.EndPoint);
            sb.AppendLine(indent + "  ScrubFields: " + this.ScrubFields);
            sb.AppendLine(indent + "  Enabled: " + this.Enabled);
            sb.AppendLine(indent + "  Environment: " + this.Environment);
            sb.AppendLine(indent + "  Server: " + this.Server);
            sb.AppendLine(indent + "  Person: " + this.Person);
            sb.AppendLine(indent + "  ProxyAddress: " + this.ProxyAddress);
            //sb.AppendLine(indent + this.Result.Trace(indent + "  "));
            return sb.ToString();
        }
    }
}
