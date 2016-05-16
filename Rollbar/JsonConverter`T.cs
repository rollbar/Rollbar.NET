using System;
using Newtonsoft.Json;

namespace Rollbar {
    public abstract class JsonConverter<T> : JsonConverter {
        public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (!(value is T)) {
                throw new JsonSerializationException(string.Format("This converter cannot convert {1} of type {0}", value, value.GetType()));
            }
            WriteJson(writer, (T)value, serializer);
        }

        public abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);

        public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (objectType != typeof (T)) {
                throw new JsonSerializationException(string.Format("This converter cannot convert type {0}", objectType));
            }
            if (!(existingValue is T)) {
                throw new JsonSerializationException(string.Format("This converter cannot convert {1} of type {0}", existingValue, existingValue.GetType()));
            }
            return ReadJson(reader, (T) existingValue, serializer);
        }

        public abstract T ReadJson(JsonReader reader, T existingValue, JsonSerializer serializer);

        public sealed override bool CanConvert(Type objectType) {
            return typeof (T) == objectType;
        }
    }
}