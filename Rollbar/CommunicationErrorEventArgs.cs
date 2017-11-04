namespace Rollbar
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class CommunicationErrorEventArgs
        : RollbarEventArgs
    {
        public CommunicationErrorEventArgs(
            RollbarConfig config, 
            Payload payload, 
            System.Exception error, 
            int retriesLeft
            ) 
            : base(config, payload)
        {
            this.Error = error;
            this.RetriesLeft = retriesLeft;
        }

        public System.Exception Error { get; private set; }

        public int RetriesLeft { get; private set; }

        public override string Trace(string indent = "")
        {
            StringBuilder sb = new StringBuilder(base.Trace(indent));
            sb.AppendLine(indent + "  RetriesLeft: " + this.RetriesLeft);
            sb.AppendLine(indent + "  Error: " + this.Error);
            return sb.ToString();
        }
    }
}
