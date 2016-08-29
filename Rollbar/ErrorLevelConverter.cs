using System;
using Newtonsoft.Json;

namespace RollbarDotNet 
{
    public class ErrorLevelConverter : JsonConverter<ErrorLevel> 
    {
        public override void WriteJson(JsonWriter writer, ErrorLevel value, JsonSerializer serializer) 
        {
            writer.WriteValue(value.ToString().ToLower());
        }

        public override ErrorLevel ReadJson(JsonReader reader, ErrorLevel existingValue, JsonSerializer serializer) 
        {
            string value;
            ErrorLevel level;
            try 
            {
                value = (string)reader.Value;
            }
            catch 
            {
                var valueType = reader.Value == null ? null : reader.Value.GetType();
                var msg = string.Format("Could not convert JsonReader value ({0}, type {1}) to an string", reader.Value, valueType);
                throw new JsonSerializationException(msg);
            }

            if (TryParse(value, out level)) 
            {
                return level;
            }

            throw new JsonSerializationException($"Could not convert {value} to an ErrorLevel");
        }

        private bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible
        {
            var retValue = value != null && Enum.IsDefined(typeof(TEnum), value);
            result = retValue ?
                        (TEnum)Enum.Parse(typeof(TEnum), value) :
                        default(TEnum);
            return retValue;
        }
    }
}
