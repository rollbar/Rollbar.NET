namespace Rollbar
{
    using Newtonsoft.Json;

    public class RollbarResponse
    {
        [JsonProperty("err")]
        public int Error { get; set; }

        public RollbarResult Result { get; set; }
    }
}
