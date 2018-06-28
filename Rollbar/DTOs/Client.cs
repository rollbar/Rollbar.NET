namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Models Rollbar Client DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    public class Client 
        : HostBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        public Client()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Client(IDictionary<string, object> arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public static class ReservedProperties
        {
        }
    }
}
