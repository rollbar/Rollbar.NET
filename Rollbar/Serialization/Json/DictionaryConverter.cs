namespace Rollbar.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DictionaryConverter
        : JsonConverter<IDictionary<string, object>>
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override IDictionary<string, object> ReadJson(JsonReader reader, IDictionary<string, object> existingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException("This library is currently not configured to fetch data from Rollbar");
        }

        public override void WriteJson(JsonWriter writer, IDictionary<string, object> value, JsonSerializer serializer)
        {
            JObject.FromObject(value, serializer).WriteTo(writer);
        }
    }
}
