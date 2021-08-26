namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarDestinationOptions.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarDestinationOptions" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarDestinationOptions" />
    public class RollbarDestinationOptions
        : ReconfigurableBase<RollbarDestinationOptions, IRollbarDestinationOptions>
        , IRollbarDestinationOptions
    {
        /// <summary>
        /// The default endpoint
        /// </summary>
        private const string defaultEndpoint = "https://api.rollbar.com/api/1/";
        /// <summary>
        /// The default environment
        /// </summary>
        private const string defaultEnvironment = "production";

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDestinationOptions"/> class.
        /// </summary>
        internal RollbarDestinationOptions()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDestinationOptions"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        public RollbarDestinationOptions(string? accessToken)
            : this(accessToken, RollbarDestinationOptions.defaultEnvironment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDestinationOptions"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="environment">The environment.</param>
        public RollbarDestinationOptions(string? accessToken, string? environment)
            : this(accessToken, environment, RollbarDestinationOptions.defaultEndpoint)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarDestinationOptions"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="endPoint">The end point.</param>
        public RollbarDestinationOptions(string? accessToken, string? environment, string endPoint)
        {
            this.AccessToken = accessToken;
            this.Environment = environment;
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string? AccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>The environment.</value>
        public string? Environment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>The end point.</value>
        public string? EndPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarDestinationOptions Reconfigure(IRollbarDestinationOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = 
                new Validator<RollbarDestinationOptions, RollbarDestinationOptions.RollbarDestinationOptionsValidationRule>()
                    .AddValidation(
                        RollbarDestinationOptions.RollbarDestinationOptionsValidationRule.ValidAccessTokenRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.AccessToken) && config.AccessToken != "seedToken"; }
                        )
                    .AddValidation(
                        RollbarDestinationOptions.RollbarDestinationOptionsValidationRule.ValidEndPointRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.EndPoint); }
                        )
                    .AddValidation(
                        RollbarDestinationOptions.RollbarDestinationOptionsValidationRule.ValidEnvironmentRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.Environment); }
                        )
               ;

            return validator;
        }

        /// <summary>
        /// Enum RollbarDestinationOptionsValidationRule
        /// </summary>
        public enum RollbarDestinationOptionsValidationRule
        {
            /// <summary>
            /// The valid end point required
            /// </summary>
            ValidEndPointRequired,

            /// <summary>
            /// The valid access token required
            /// </summary>
            ValidAccessTokenRequired,

            /// <summary>
            /// The valid environment required
            /// </summary>
            ValidEnvironmentRequired,
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarDestinationOptions IReconfigurable<IRollbarDestinationOptions, IRollbarDestinationOptions>.Reconfigure(IRollbarDestinationOptions likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
