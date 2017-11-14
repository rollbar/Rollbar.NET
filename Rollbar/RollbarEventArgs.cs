namespace Rollbar
{
    using Newtonsoft.Json;
    using Rollbar.DTOs;
    using System;
    using System.Text;

    /// <summary>
    /// An abstract base for implementing concrete Rollbar EventArgs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    /// <seealso cref="Rollbar.ITraceable" />
    public abstract class RollbarEventArgs 
        : EventArgs
        , ITraceable
    {
        private RollbarEventArgs()
        {

        }

        protected RollbarEventArgs(
            RollbarConfig config, 
            Payload payload
            )
        {
            this.Config = config;
            if (payload != null)
            {
                this.Payload = JsonConvert.SerializeObject(payload);
            }
            //else
            //{
            //    this.Payload = string.Empty;
            //}
        }

        public RollbarConfig Config { get; private set; }
        public string Payload { get; private set; }

        public virtual string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.Append(indent + this.Config.TraceAsString("  "));
            sb.AppendLine(indent + "  Payload: " + this.Payload);
            return sb.ToString();
        }
    }
}
