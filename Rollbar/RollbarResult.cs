namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class RollbarResult
        : ITraceable
    {
        public int? Id { get; set; }

        public string Uuid { get; set; }

        public string Trace(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  Id: " + this.Id);
            sb.AppendLine(indent + "  Uuid: " + this.Uuid);
            return sb.ToString();
        }
    }
}
