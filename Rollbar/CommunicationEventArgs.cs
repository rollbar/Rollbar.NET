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

        public override string Trace(string indent = "")
        {
            StringBuilder sb = new StringBuilder(base.Trace(indent));
            sb.AppendLine(indent + "  Response: " );
            sb.AppendLine(this.Response.Trace(indent + "  "));
            return sb.ToString();
        }
    }
}
