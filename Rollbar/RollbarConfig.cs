namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class RollbarConfig
        : ReconfigurableBase<RollbarConfig>
        , ITraceable
    {
        private readonly RollbarLogger _logger = null;

        private RollbarConfig()
        {
        }

        internal RollbarConfig(RollbarLogger logger)
        {
            this._logger = logger;
        }

        internal RollbarLogger Logger
        {
            get { return this._logger; }
        }

        public RollbarConfig(string accessToken)
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));

            this.AccessToken = accessToken;

            // let's set some default values:
            this.Environment = "production";
            this.Enabled = true;
            this.MaxReportsPerMinute = 60;
            this.ReportingQueueDepth = 20;
            this.LogLevel = ErrorLevel.Debug;
            this.ScrubFields = new[] 
            {
                "passwd",
                "password",
                "secret",
                "confirm_password",
                "password_confirmation",
            };
            this.EndPoint = "https://api.rollbar.com/api/1/";
            this.ProxyAddress = null;
            this.CheckIgnore = null;
            this.Transform = null;
            this.Truncate = null;
            this.Server = null;
            this.Person = null;
        }

        public string AccessToken { get; internal set; }

        public string EndPoint { get; set; }

        public string[] ScrubFields { get; set; }

        public ErrorLevel? LogLevel { get; set; }

        public bool? Enabled { get; set; }

        public string Environment { get; set; }

        public Func<Payload, bool> CheckIgnore { get; set; }

        public Action<Payload> Transform { get; set; }

        public Action<Payload> Truncate { get; set; }

        public Server Server { get; set; }

        public Person Person { get;set; }

        public string ProxyAddress { get; set; }

        public int MaxReportsPerMinute { get; set; }

        public int ReportingQueueDepth { get; set; }

        public string TraceAsString(string indent = "")
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
            sb.AppendLine(indent + "  MaxReportsPerMinute: " + this.MaxReportsPerMinute);
            //sb.AppendLine(indent + this.Result.Trace(indent + "  "));
            return sb.ToString();
        }
    }
}
