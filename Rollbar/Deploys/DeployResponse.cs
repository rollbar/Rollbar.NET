namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DeployResponse
        : ResponseBase
    {
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Deploy Deploy { get; set; }
    }
}
