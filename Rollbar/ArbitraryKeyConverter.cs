using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RollbarDotNet 
{
    public class ArbitraryKeyConverter : JsonConverter<HasArbitraryKeys> 
    {
        public override void WriteJson(JsonWriter writer, HasArbitraryKeys value, JsonSerializer serializer) 
        {
            JObject.FromObject(value.Denormalize(), serializer).WriteTo(writer);
        }

        public override HasArbitraryKeys ReadJson(JsonReader reader, HasArbitraryKeys existingValue, JsonSerializer serializer) 
        {
            throw new InvalidOperationException("This library is currently not configured to fetch data from Rollbar");
        }

        public override bool CanRead 
        {
            get { return false; }
        }
    }
}