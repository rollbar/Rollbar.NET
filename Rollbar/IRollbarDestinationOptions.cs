namespace Rollbar
{
    using System;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarDestinationOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T,TBase}" />
    public interface IRollbarDestinationOptions
        : IReconfigurable<IRollbarDestinationOptions, IRollbarDestinationOptions>
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>The access token.</value>
        string? AccessToken
        {
            get;
        }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>The environment.</value>
        string? Environment
        {
            get;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>The end point.</value>
        string? EndPoint
        {
            get;
        }

    }
}
