namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Interface IRollbarPayloadAdditionOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    public interface IRollbarPayloadAdditionOptions
        : IReconfigurable<IRollbarPayloadAdditionOptions, IRollbarPayloadAdditionOptions>
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        Person? Person
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        Server? Server
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets the code version.
        /// </summary>
        /// <value>
        /// The code version.
        /// </value>
        public string? CodeVersion
        {
            get;
            set;
        }
    }
}
