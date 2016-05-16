using System;
using Newtonsoft.Json;

namespace Rollbar {
    public class ErrorLevelConverter : JsonConverter<ErrorLevel> {
        public override void WriteJson(JsonWriter writer, ErrorLevel value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString().ToLower());
        }

        public override ErrorLevel ReadJson(JsonReader reader, ErrorLevel existingValue, JsonSerializer serializer) {
            string value;
            ErrorLevel level;
            try {
                value = (string) reader.Value;
            }
            catch {
                var valueType = reader.Value == null ? null : reader.Value.GetType();
                var msg = string.Format("Could not convert JsonReader value ({0}, type {1}) to an string", reader.Value, valueType);
                throw new JsonSerializationException(msg);
            }
            if (Enum.TryParse(value, true, out level)) {
                return level;
            }
            throw new JsonSerializationException(string.Format("Could not convert {0} to an ErrorLevel", value));
        }
    }
}
