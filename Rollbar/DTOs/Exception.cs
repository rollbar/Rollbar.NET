namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    public class Exception
        : DtoBase
    {
        public Exception(string @class)
        {
            Class = @class;
        }

        public Exception(System.Exception exception)
        {
            Assumption.AssertNotNull(exception, nameof(exception));

            Class = exception.GetType().FullName;
            Message = exception.Message;
        }

        [JsonProperty("class", Required = Required.Always)]
        public string Class { get; private set; }

        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
