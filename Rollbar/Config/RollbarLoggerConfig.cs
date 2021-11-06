namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Newtonsoft.Json;

    using Rollbar.Common;
    using Rollbar.Infrastructure;
    using Rollbar.NetStandard;

    /// <summary>
    /// Class RollbarLoggerConfig.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarLoggerConfig" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarLoggerConfig" />
    public class RollbarLoggerConfig
        : ReconfigurableBase<RollbarLoggerConfig, IRollbarLoggerConfig>
        , IRollbarLoggerConfig
    {
        /// <summary>
        /// The rollbar destination options
        /// </summary>
        private readonly RollbarDestinationOptions _rollbarDestinationOptions = 
            new RollbarDestinationOptions();
        /// <summary>
        /// The HTTP proxy options
        /// </summary>
        private readonly HttpProxyOptions _httpProxyOptions = 
            new HttpProxyOptions();
        /// <summary>
        /// The rollbar developer options
        /// </summary>
        private readonly RollbarDeveloperOptions _rollbarDeveloperOptions = 
            new RollbarDeveloperOptions();
        /// <summary>
        /// The rollbar data security options
        /// </summary>
        private readonly RollbarDataSecurityOptions _rollbarDataSecurityOptions = 
            new RollbarDataSecurityOptions();
        /// <summary>
        /// The rollbar payload addition options
        /// </summary>
        private readonly RollbarPayloadAdditionOptions _rollbarPayloadAdditionOptions = 
            new RollbarPayloadAdditionOptions();
        /// <summary>
        /// The rollbar payload manipulation options
        /// </summary>
        private readonly RollbarPayloadManipulationOptions _rollbarPayloadManipulationOptions = 
            new RollbarPayloadManipulationOptions();

        /// <summary>
        /// The logger
        /// </summary>
        private readonly IRollbar? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerConfig" /> class.
        /// </summary>
        public RollbarLoggerConfig()
            : this(null as string)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerConfig" /> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        public RollbarLoggerConfig(string? accessToken, string? rollbarEnvironment = null)
        {
            this.SetDefaults();

            if(!string.IsNullOrWhiteSpace(accessToken))
            {
                this._rollbarDestinationOptions.AccessToken = accessToken;
                if(!string.IsNullOrWhiteSpace(rollbarEnvironment))
                {
                    this._rollbarDestinationOptions.Environment = rollbarEnvironment;
                }
            }
            else
            {
                // initialize based on application configuration file (if any):
                var configLoader = new RollbarConfigurationLoader();
                RollbarInfrastructureConfig config = new RollbarInfrastructureConfig();
                configLoader.Load(config);
                this.Reconfigure(config.RollbarLoggerConfig);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerConfig"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        internal RollbarLoggerConfig(IRollbar logger)
        {
            this._logger = logger;

            this.SetDefaults();

            // initialize based on application configuration file (if any):
            var configLoader = new RollbarConfigurationLoader();
            RollbarInfrastructureConfig config = new RollbarInfrastructureConfig();
            configLoader.Load(config);
            this.Reconfigure(config.RollbarLoggerConfig);
        }

        /// <summary>
        /// Sets the defaults.
        /// </summary>
        private void SetDefaults()
        {
            this._rollbarDestinationOptions.EndPoint = "https://api.rollbar.com/api/1/";
            this._rollbarDestinationOptions.Environment = "production";

            this._rollbarDeveloperOptions.LogLevel = ErrorLevel.Debug;
            this._rollbarDeveloperOptions.Enabled = true;
            this._rollbarDeveloperOptions.Transmit = true;
            this._rollbarDeveloperOptions.RethrowExceptionsAfterReporting = false;
            this._rollbarDeveloperOptions.WrapReportedExceptionWithRollbarException = true;

            this._httpProxyOptions.ProxyAddress = null;
            this._httpProxyOptions.ProxyUsername = null;
            this._httpProxyOptions.ProxyPassword = null;

            this._rollbarPayloadManipulationOptions.CheckIgnore = null;
            this._rollbarPayloadManipulationOptions.Transform = null;
            this._rollbarPayloadManipulationOptions.Truncate = null;

            this._rollbarPayloadAdditionOptions.Server = null;
            this._rollbarPayloadAdditionOptions.Person = null;

            this._rollbarDataSecurityOptions.ScrubFields = RollbarDataScrubbingHelper.Instance.GetDefaultFields().ToArray();
            this._rollbarDataSecurityOptions.ScrubSafelistFields = new string[] { };
            this._rollbarDataSecurityOptions.PersonDataCollectionPolicies = PersonDataCollectionPolicies.None;
            this._rollbarDataSecurityOptions.IpAddressCollectionPolicy = IpAddressCollectionPolicy.Collect;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        internal IRollbar? Logger
        {
            get
            {
                return this._logger;
            }
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarLoggerConfig Reconfigure(IRollbarLoggerConfig likeMe)
        {
            base.Reconfigure(likeMe);

            RollbarLogger? rollbarLogger = this.Logger as RollbarLogger;

            if(rollbarLogger != null && rollbarLogger.Queue != null)
            {
                var rollbarClient = new RollbarClient(rollbarLogger);
                // reset the queue to use the new RollbarClient:
                rollbarLogger.Queue.Flush();
                rollbarLogger.Queue.UpdateClient(rollbarClient);
            }

            return this;
        }

        /// <summary>
        /// Gets the rollbar destination options.
        /// </summary>
        /// <value>The rollbar destination options.</value>
        public IRollbarDestinationOptions RollbarDestinationOptions
        {
            get
            {
                return this._rollbarDestinationOptions;
            }
        }

        /// <summary>
        /// Gets the HTTP proxy options.
        /// </summary>
        /// <value>The HTTP proxy options.</value>
        public IHttpProxyOptions HttpProxyOptions
        {
            get
            {
                return this._httpProxyOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar developer options.
        /// </summary>
        /// <value>The rollbar developer options.</value>
        public IRollbarDeveloperOptions RollbarDeveloperOptions
        {
            get
            {
                return this._rollbarDeveloperOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar data security options.
        /// </summary>
        /// <value>The rollbar data security options.</value>
        public IRollbarDataSecurityOptions RollbarDataSecurityOptions
        {
            get
            {
                return this._rollbarDataSecurityOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar payload addition options.
        /// </summary>
        /// <value>The rollbar payload addition options.</value>
        public IRollbarPayloadAdditionOptions RollbarPayloadAdditionOptions
        {
            get
            {
                return this._rollbarPayloadAdditionOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar payload manipulation options.
        /// </summary>
        /// <value>The rollbar payload manipulation options.</value>
        [JsonIgnore]
        public IRollbarPayloadManipulationOptions RollbarPayloadManipulationOptions
        {
            get
            {
                return this._rollbarPayloadManipulationOptions;
            }
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = 
                new Validator<RollbarLoggerConfig, RollbarLoggerConfig.RollbarLoggerConfigValidationRule>()
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.DestinationOptionsRequired,
                        (config) => { return this.RollbarDestinationOptions != null; }
                        )
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.HttpProxyOptionsRequred,
                        (config) => { return this.HttpProxyOptions != null; }
                        )
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.DeveloperOptionsRequired,
                        (config) => { return this.RollbarDeveloperOptions != null; }
                        )
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.DataSecurityOptionsRequired,
                        (config) => { return this.RollbarDataSecurityOptions != null; }
                        )
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.PayloadAdditionOptionsRequired,
                        (config) => { return this.RollbarPayloadAdditionOptions != null; }
                        )
                    .AddValidation(
                        RollbarLoggerConfig.RollbarLoggerConfigValidationRule.PayloadManipulationOptionsRequired,
                        (config) => { return this.RollbarPayloadManipulationOptions != null; }
                        )
               ;

            IValidatable[] validatableComponents =
            {
                this._rollbarDestinationOptions,
                this._httpProxyOptions,
                this._rollbarDeveloperOptions,
                this._rollbarDataSecurityOptions,
                this._rollbarPayloadAdditionOptions,
                this._rollbarPayloadManipulationOptions,
            };

            Debug.Assert(validator.TotalValidationRules == (new HashSet<IValidatable>(validatableComponents)).Count);
            CompositeValidator compositeValidator = 
                new CompositeValidator(
                    new Validator[] { validator },
                    validatableComponents
                    );

            return compositeValidator;
        }

        /// <summary>
        /// Enum RollbarLoggerConfigValidationRule
        /// </summary>
        public enum RollbarLoggerConfigValidationRule
        {
            /// <summary>
            /// The destination options required
            /// </summary>
            DestinationOptionsRequired,
            /// <summary>
            /// The HTTP proxy options requred
            /// </summary>
            HttpProxyOptionsRequred,
            /// <summary>
            /// The developer options required
            /// </summary>
            DeveloperOptionsRequired,
            /// <summary>
            /// The data security options required
            /// </summary>
            DataSecurityOptionsRequired,
            /// <summary>
            /// The payload addition options required
            /// </summary>
            PayloadAdditionOptionsRequired,
            /// <summary>
            /// The payload manipulation options required
            /// </summary>
            PayloadManipulationOptionsRequired,
        }

        #region IRollbarLoggerConfig

        /// <summary>
        /// Gets the rollbar destination options.
        /// </summary>
        /// <value>The rollbar destination options.</value>
        IRollbarDestinationOptions IRollbarLoggerConfig.RollbarDestinationOptions
        {
            get
            {
                return this.RollbarDestinationOptions;
            }
        }
        /// <summary>
        /// Gets the HTTP proxy options.
        /// </summary>
        /// <value>The HTTP proxy options.</value>
        IHttpProxyOptions IRollbarLoggerConfig.HttpProxyOptions
        {
            get
            {
                return this.HttpProxyOptions;
            }
        }
        /// <summary>
        /// Gets the rollbar developer options.
        /// </summary>
        /// <value>The rollbar developer options.</value>
        IRollbarDeveloperOptions IRollbarLoggerConfig.RollbarDeveloperOptions
        {
            get
            {
                return this.RollbarDeveloperOptions;
            }
        }
        /// <summary>
        /// Gets the rollbar data security options.
        /// </summary>
        /// <value>The rollbar data security options.</value>
        IRollbarDataSecurityOptions IRollbarLoggerConfig.RollbarDataSecurityOptions
        {
            get
            {
                return this.RollbarDataSecurityOptions;
            }
        }
        /// <summary>
        /// Gets the rollbar payload addition options.
        /// </summary>
        /// <value>The rollbar payload addition options.</value>
        IRollbarPayloadAdditionOptions IRollbarLoggerConfig.RollbarPayloadAdditionOptions
        {
            get
            {
                return this.RollbarPayloadAdditionOptions;
            }
        }
        /// <summary>
        /// Gets the rollbar payload manipulation options.
        /// </summary>
        /// <value>The rollbar payload manipulation options.</value>
        IRollbarPayloadManipulationOptions IRollbarLoggerConfig.RollbarPayloadManipulationOptions
        {
            get
            {
                return this.RollbarPayloadManipulationOptions;
            }
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarLoggerConfig IReconfigurable<IRollbarLoggerConfig, IRollbarLoggerConfig>.Reconfigure(IRollbarLoggerConfig likeMe)
        {
            return this.Reconfigure(likeMe);
        }

        /// <summary>
        /// Occurs when this instance reconfigured.
        /// </summary>
        event EventHandler IReconfigurable<IRollbarLoggerConfig, IRollbarLoggerConfig>.Reconfigured
        {
            add
            {
                this.Reconfigured += value;
            }

            remove
            {
                this.Reconfigured -= value;
            }
        }

        #endregion IRollbarLoggerConfig

    }
}
