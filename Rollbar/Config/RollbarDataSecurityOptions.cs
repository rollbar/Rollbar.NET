namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarDataSecurityOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarDataSecurityOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarDataSecurityOptions" />
    public class RollbarDataSecurityOptions
        : ReconfigurableBase<RollbarDataSecurityOptions, IRollbarDataSecurityOptions>
        , IRollbarDataSecurityOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDataSecurityOptions"/> class.
        /// </summary>
        public RollbarDataSecurityOptions()
            : this(
                  PersonDataCollectionPolicies.None, 
                  IpAddressCollectionPolicy.Collect,
                  RollbarDataScrubbingHelper.Instance.GetDefaultFields().ToArray(),
                  null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDataSecurityOptions"/> class.
        /// </summary>
        /// <param name="personDataCollectionPolicies">The person data collection policies.</param>
        /// <param name="ipAddressCollectionPolicy">The ip address collection policy.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubSafeFields">The scrub safe fields.</param>
        public RollbarDataSecurityOptions(
            PersonDataCollectionPolicies personDataCollectionPolicies, 
            IpAddressCollectionPolicy ipAddressCollectionPolicy,
            string[]? scrubFields = null,
            string[]? scrubSafeFields = null)
        {
            this.PersonDataCollectionPolicies = personDataCollectionPolicies;
            this.IpAddressCollectionPolicy = ipAddressCollectionPolicy;
            this.ScrubFields = scrubFields ?? new string[0];
            this.ScrubSafelistFields = scrubSafeFields ?? new string[0];
        }

        /// <summary>
        /// Gets or sets the person data collection policies.
        /// </summary>
        /// <value>The person data collection policies.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonDataCollectionPolicies PersonDataCollectionPolicies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IP address collection policy.
        /// </summary>
        /// <value>The IP address collection policy.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public IpAddressCollectionPolicy IpAddressCollectionPolicy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the scrub fields.
        /// </summary>
        /// <value>The scrub fields.</value>
        public string[] ScrubFields
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the scrub safelist fields.
        /// </summary>
        /// <value>The scrub safelist fields.</value>
        /// <remarks>The fields mentioned in this list are guaranteed to be excluded
        /// from the ScrubFields list in cases when the lists overlap.</remarks>
        public string[] ScrubSafelistFields
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the fields to scrub.
        /// </summary>
        /// <returns>Actual fields to be scrubbed based on combining the ScrubFields with the ScrubSafelistFields.</returns>
        public IReadOnlyCollection<string> GetFieldsToScrub()
        {
            if(this.ScrubFields == null || this.ScrubFields.Length == 0)
            {
                return new string[0];
            }

            if(this.ScrubSafelistFields == null || this.ScrubSafelistFields.Length == 0)
            {
                return this.ScrubFields.ToArray();
            }

            var scrubSafeList = this.ScrubSafelistFields.ToArray();
            return this.ScrubFields.Where(i => !scrubSafeList.Contains(i)).ToArray();
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarDataSecurityOptions Reconfigure(IRollbarDataSecurityOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator? GetValidator()
        {
            return null;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarDataSecurityOptions IReconfigurable<IRollbarDataSecurityOptions, IRollbarDataSecurityOptions>.Reconfigure(IRollbarDataSecurityOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
