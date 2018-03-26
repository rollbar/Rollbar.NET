using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.Deploys
{
    public abstract class ResponseBase
    {
        [JsonProperty("err", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ErrorCode { get; set; }
    }
}
