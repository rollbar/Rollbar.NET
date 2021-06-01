namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.DTOs;

    public interface IRollbarPayloadManipulationOptions
    {
        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        Action<Payload> Transform
        {
            get;
        }

        /// <summary>
        /// Gets the truncate.
        /// </summary>
        /// <value>
        /// The truncate.
        /// </value>
        Action<Payload> Truncate
        {
            get;
        }

        /// <summary>
        /// Gets the check ignore.
        /// </summary>
        /// <value>
        /// The check ignore.
        /// </value>
        Func<Payload,bool> CheckIgnore
        {
            get;
        }
    }
}
