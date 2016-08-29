using System;
using Newtonsoft.Json;

namespace RollbarDotNet 
{
    public class Payload 
    {
        public Payload(string accessToken, Data data) 
        {
            if (string.IsNullOrEmpty(accessToken?.Trim())) 
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (data == null) 
            {
                throw new ArgumentNullException(nameof(data));
            }

            AccessToken = accessToken;
            Data = data;
        }

        public string ToJson() 
        {
            return JsonConvert.SerializeObject(this);
        }

        [JsonProperty("access_token", Required = Required.Always)]
        public string AccessToken { get; private set; }

        [JsonProperty("data", Required = Required.Always)]
        public Data Data { get; private set; }
    }
}
