namespace Rollbar
{
    using System;

    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Interface IRollbarPayloadManipulationOptions
    /// Implements the <see cref="IReconfigurable{T, TBase}" />
    /// </summary>
    /// <seealso cref="IReconfigurable{T, TBase}" />
    public interface IRollbarPayloadManipulationOptions
        : IReconfigurable<IRollbarPayloadManipulationOptions, IRollbarPayloadManipulationOptions>
    {
        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        Action<Payload>? Transform
        {
            get;
        }

        /// <summary>
        /// Gets the truncate.
        /// </summary>
        /// <value>
        /// The truncate.
        /// </value>
        Action<Payload>? Truncate
        {
            get;
        }

        /// <summary>
        /// Gets the check ignore.
        /// </summary>
        /// <value>
        /// The check ignore.
        /// </value>
        Func<Payload,bool>? CheckIgnore
        {
            get;
        }
    }
}
