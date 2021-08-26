namespace Rollbar.DTOs
{
    using System.Collections.Generic;

    /// <summary>
    /// Models Rollbar Client DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.HostBase" />
    /// <remarks>
    ///  Optional: client
    /// Data about the client device this event occurred on.
    /// As there can be multiple client environments for a given event (i.e. Flash running inside
    /// an HTML page), data should be namespaced by platform.
    /// </remarks>
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
        public Client(IDictionary<string, object?>? arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public new static class ReservedProperties
        {
        }
    }
}
