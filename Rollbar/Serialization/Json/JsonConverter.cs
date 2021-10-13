namespace Rollbar.Serialization.Json
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Abstract base for implementing Json converters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public abstract class JsonConverter<T> 
        : JsonConverter
    {

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <exception cref="JsonSerializationException"></exception>
        public sealed override void WriteJson(
            JsonWriter writer, 
            object? value, 
            JsonSerializer serializer
            )
        {
            if (!(value is T))
            {
                throw new JsonSerializationException(string.Format("This converter cannot convert {1} of type {0}", value, value?.GetType()));
            }

            WriteJson(writer, (T)value, serializer);
        }

        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        /// <exception cref="JsonSerializationException">
        /// </exception>
        public sealed override object? ReadJson(
            JsonReader reader, 
            Type objectType, 
            object? existingValue, 
            JsonSerializer serializer
            )
        {
            if (objectType != typeof(T) 
                && !(objectType.IsGenericType && objectType.GenericTypeArguments.Length == 1 && objectType.GenericTypeArguments[0] == typeof(T))
                )
            {
                throw new JsonSerializationException(string.Format("This converter cannot convert type {0}", objectType));
            }

            if (!(existingValue is T))
            {
                throw new JsonSerializationException(string.Format("This converter cannot convert {1} of type {0}", existingValue, existingValue?.GetType()));
            }

            return ReadJson(reader, (T) existingValue, serializer);
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        public abstract T ReadJson(JsonReader reader, T existingValue, JsonSerializer serializer);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public sealed override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }
    }
}