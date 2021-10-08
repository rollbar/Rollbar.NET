namespace Rollbar.Serialization.Json
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// ErrorLevelConverter simplifies ErrorLevel Json de/serialization.
    /// </summary>
    /// <seealso cref="Rollbar.Serialization.Json.JsonConverter{T}" />
    public class ErrorLevelConverter 
        : JsonConverter<ErrorLevel>
    {

        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(
            JsonWriter writer, 
            ErrorLevel value, 
            JsonSerializer serializer
            )
        {
            writer.WriteValue(value.ToString().ToLower());
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        /// <exception cref="JsonSerializationException">
        /// </exception>
        public override ErrorLevel ReadJson(
            JsonReader reader, 
            ErrorLevel existingValue, 
            JsonSerializer serializer
            )
        {
            string? value = reader.Value as string;

            if (string.IsNullOrWhiteSpace(value))
            {
                var valueType = reader.Value == null ? null : reader.Value.GetType();
                var msg = string.Format("Could not convert JsonReader value ({0}, type {1}) to an string", reader.Value, valueType);
                throw new JsonSerializationException(msg);
            }
            else
            {
                char[]? chars = value?.ToCharArray();
                if (chars != null && chars.Length > 0)
                {
                    chars[0] = Char.ToUpper(chars[0]);
                    value = new string(chars);

                    if (Enum.TryParse(value, true, out ErrorLevel level))
                    {
                        return level;
                    }
                }

                throw new JsonSerializationException(string.Format("Could not convert {0} to an ErrorLevel", value));
            }
        }
    }
}