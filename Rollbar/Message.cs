using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RollbarDotNet 
{
    [JsonConverter(typeof(ArbitraryKeyConverter))]
    public class Message : HasArbitraryKeys 
    {
        public Message(string body) 
        {
            if (string.IsNullOrEmpty(body?.Trim())) 
            {
                throw new ArgumentNullException(nameof(body));
            }

            Body = body;
        }

        public string Body { get; private set; }

        protected override void Normalize() 
        {
            Body = (string)(AdditionalKeys.ContainsKey("body") && !string.IsNullOrEmpty((AdditionalKeys["body"] as string)?.Trim()) ? AdditionalKeys["body"] : Body);
            AdditionalKeys.Remove("body");
        }

        protected override Dictionary<string, object> Denormalize(Dictionary<string, object> dict) 
        {
            dict["body"] = Body;
            return dict;
        }
    }
}
