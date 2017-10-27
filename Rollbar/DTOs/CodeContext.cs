namespace Rollbar.DTOs
{
    using Newtonsoft.Json;

    public class CodeContext
        : DtoBase
    {
        [JsonProperty("pre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Pre { get; set; }

        [JsonProperty("post", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Post { get; set; }
    }
}
