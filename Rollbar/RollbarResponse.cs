using Newtonsoft.Json;

namespace RollbarDotNet
{
    public class RollbarResponse
    {
        [JsonProperty("err")]
        public int Error { get; set; }

        public RollbarResult Result { get; set; }
    }
}