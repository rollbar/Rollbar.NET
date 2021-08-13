namespace Rollbar.PayloadStore
{
    using System;

    /// <summary>
    /// Interface IDestination
    /// </summary>
    public interface IDestination
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        /// <value>The endpoint.</value>
        string? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        Guid ID { get; set; }
    }
}