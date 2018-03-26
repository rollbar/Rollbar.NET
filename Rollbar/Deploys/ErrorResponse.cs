namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;


    public class ErrorResponse
        : ResponseBase
    {
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; set; }
    }
}
