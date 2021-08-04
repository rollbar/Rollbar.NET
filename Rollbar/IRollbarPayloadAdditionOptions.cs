namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Interface IRollbarPayloadAdditionOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{Rollbar.IRollbarPayloadAdditionOptions, Rollbar.IRollbarPayloadAdditionOptions}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{Rollbar.IRollbarPayloadAdditionOptions, Rollbar.IRollbarPayloadAdditionOptions}" />
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
    }
}
