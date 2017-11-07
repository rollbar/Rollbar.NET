namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.DTOs;

    public class CommunicationEventArgs
        : RollbarEventArgs
    {
        public CommunicationEventArgs(RollbarConfig config, Payload payload, RollbarResponse response) 
            : base(config, payload)
        {
            this.Response = response;
        }

        public RollbarResponse Response { get; private set; }

        public override string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder(base.TraceAsString(indent));
            sb.AppendLine(indent + "  Response: " );
            sb.AppendLine(this.Response.TraceAsString(indent + "  "));
            return sb.ToString();
        }
    }
}
