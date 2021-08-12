namespace Rollbar
{
    using System;

    using Newtonsoft.Json;

    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarPayloadManipulationOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarPayloadManipulationOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarPayloadManipulationOptions" />
    public class RollbarPayloadManipulationOptions
        : ReconfigurableBase<RollbarPayloadManipulationOptions, IRollbarPayloadManipulationOptions>
        , IRollbarPayloadManipulationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPayloadManipulationOptions"/> class.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="truncate">The truncate.</param>
        /// <param name="checkIgnore">The check ignore.</param>
        public RollbarPayloadManipulationOptions(
            Action<Payload>? transform = null, 
            Action<Payload>? truncate = null, 
            Func<Payload, bool>? checkIgnore = null)
        {
            Transform = transform;
            Truncate = truncate;
            CheckIgnore = checkIgnore;
        }

        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <value>The transform.</value>
        [JsonIgnore]
        public Action<Payload>? Transform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the truncate.
        /// </summary>
        /// <value>The truncate.</value>
        [JsonIgnore]
        public Action<Payload>? Truncate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the check ignore.
        /// </summary>
        /// <value>The check ignore.</value>
        [JsonIgnore]
        public Func<Payload, bool>? CheckIgnore
        {
            get;
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarPayloadManipulationOptions Reconfigure(IRollbarPayloadManipulationOptions likeMe)
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
        /// Traces as a string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>String rendering of this instance.</returns>
        public override string TraceAsString(string indent)
        {
            return base.TraceAsString(indent);
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarPayloadManipulationOptions IReconfigurable<IRollbarPayloadManipulationOptions, IRollbarPayloadManipulationOptions>.Reconfigure(IRollbarPayloadManipulationOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
