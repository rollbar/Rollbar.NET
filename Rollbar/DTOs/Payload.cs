namespace Rollbar.DTOs
{
    using System;
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    public class Payload
        : DtoBase
    {
        public Payload(string accessToken, Data data)
        {
            AccessToken = accessToken;
            Data = data;
            Validate();
        }

        //public string ToJson()
        //{
        //    return JsonConvert.SerializeObject(this);
        //}

        [JsonProperty("access_token", Required = Required.Always)]
        public string AccessToken { get; private set; }

        [JsonProperty("data", Required = Required.Always)]
        public Data Data { get; private set; }

        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.AccessToken, nameof(this.AccessToken));
            Assumption.AssertNotNull(this.Data, nameof(this.Data));

            this.Data.Validate();
        }
    }
}
