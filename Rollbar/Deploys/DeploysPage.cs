namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DeploysPage
    {
        [JsonProperty("page", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int PageNumber { get; set; }

        [JsonProperty("deploys", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Deploy[] Deploys { get; set; }
    }
}
