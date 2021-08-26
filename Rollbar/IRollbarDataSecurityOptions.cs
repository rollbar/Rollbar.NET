namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    /// <summary>
    /// Interface IRollbarDataSecurityOptions
    /// Implements the <see cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.IReconfigurable{T, TBase}" />
    public interface IRollbarDataSecurityOptions
        :IReconfigurable<IRollbarDataSecurityOptions, IRollbarDataSecurityOptions>
    {
        /// <summary>
        /// Gets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        string[] ScrubFields
        {
            get;
        }

        /// <summary>
        /// Gets the scrub safelist fields.
        /// </summary>
        /// <value>The scrub safelist fields.</value>
        /// <remarks>
        /// The fields mentioned in this list are guaranteed to be excluded
        /// from the ScrubFields list in cases when the lists overlap.
        /// </remarks>
        string[] ScrubSafelistFields
        {
            get;
        }

        /// <summary>
        /// Gets the fields to scrub.
        /// </summary>
        /// <returns>
        /// Actual fields to be scrubbed based on combining the ScrubFields with the ScrubSafelistFields.
        /// </returns>
        IReadOnlyCollection<string> GetFieldsToScrub();

        /// <summary>
        /// Gets or sets the person data collection policies.
        /// </summary>
        /// <value>
        /// The person data collection policies.
        /// </value>
        PersonDataCollectionPolicies PersonDataCollectionPolicies
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the IP address collection policy.
        /// </summary>
        /// <value>
        /// The IP address collection policy.
        /// </value>
        IpAddressCollectionPolicy IpAddressCollectionPolicy
        {
            get; set;
        }
    }
}
