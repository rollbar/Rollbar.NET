namespace Rollbar
{
    using System.Text;
    using Rollbar.DTOs;

    public class InternalErrorEventArgs
        : RollbarEventArgs
    {
        public InternalErrorEventArgs(
            RollbarConfig config,
            Payload payload,
            System.Exception error,
            string details
            ) 
            : base(config, payload)
        {
            this.Error = error;
            this.Details = details;
        }

        public System.Exception Error { get; private set; }
        public string Details { get; private set; }

        public override string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder(base.TraceAsString(indent));
            sb.AppendLine(indent + "  Error: " + this.Error);
            sb.AppendLine(indent + "  Details: " + this.Details);
            return sb.ToString();
        }
    }
}
