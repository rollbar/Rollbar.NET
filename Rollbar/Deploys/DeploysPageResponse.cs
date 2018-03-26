namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DeploysPageResponse
        : ResponseBase
    {
        [JsonProperty("result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeploysPage DeploysPage { get; set; }
    }
}
