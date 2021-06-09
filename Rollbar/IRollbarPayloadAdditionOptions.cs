namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;
    using Rollbar.DTOs;

    public interface IRollbarPayloadAdditionOptions
        : IReconfigurable<IRollbarPayloadAdditionOptions, IRollbarPayloadAdditionOptions>
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        Person Person
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
        Server Server
        {
            get; 
            set;
        }
    }
}
