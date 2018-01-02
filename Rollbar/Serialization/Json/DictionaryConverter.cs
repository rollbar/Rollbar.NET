namespace Rollbar.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
#pragma warning disable CS1658 // Warning is overriding an error
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// DictionaryConverter simplifies Json de/serialization of .NET Dictionaries.
    /// </summary>
    /// <seealso cref="Json.JsonConverter{System.Collections.Generic.IDictionary{System.String, System.Object}}" />
    public class DictionaryConverter
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
        : JsonConverter<IDictionary<string, object>>
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">This library is currently not configured to fetch data from Rollbar</exception>
        public override IDictionary<string, object> ReadJson(
            JsonReader reader, 
            IDictionary<string, object> existingValue, 
            JsonSerializer serializer
            )
        {
            throw new InvalidOperationException("This library is currently not configured to fetch data from Rollbar");
        }

        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(
            JsonWriter writer, 
            IDictionary<string, object> value, 
            JsonSerializer serializer
            )
        {
            JObject.FromObject(value, serializer).WriteTo(writer);
        }
    }
}
