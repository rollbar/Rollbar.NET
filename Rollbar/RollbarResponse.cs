namespace Rollbar
{
    using Newtonsoft.Json;
    using System.Text;

    public class RollbarResponse
        : ITraceable
    {
        [JsonProperty("err")]
        public int Error { get; set; }

        public RollbarResult Result { get; set; }

        public string TraceAsString(string indent = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  Error: " + this.Error);
            sb.AppendLine(indent + "  Result: ");
            sb.AppendLine(indent  + this.Result.TraceAsString(indent + "  "));
            return sb.ToString();
        }
    }
}
