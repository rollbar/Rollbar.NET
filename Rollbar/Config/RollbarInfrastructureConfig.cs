namespace Rollbar
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using Rollbar.Common;

    /// <summary>
    /// Class RollbarInfrastructureConfig.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// Implements the <see cref="Rollbar.IRollbarInfrastructureConfig" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{T,TBase}" />
    /// <seealso cref="Rollbar.IRollbarInfrastructureConfig" />
    public class RollbarInfrastructureConfig
        : ReconfigurableBase<RollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IRollbarInfrastructureConfig

    {
        /// <summary>
        /// The rollbar logger configuration
        /// </summary>
        private readonly RollbarLoggerConfig _rollbarLoggerConfig;

        /// <summary>
        /// The rollbar infrastructure options
        /// </summary>
        private readonly RollbarInfrastructureOptions _rollbarInfrastructureOptions = 
            new RollbarInfrastructureOptions();
        /// <summary>
        /// The rollbar telemetry options
        /// </summary>
        private readonly RollbarTelemetryOptions _rollbarTelemetryOptions = 
            new RollbarTelemetryOptions();
        /// <summary>
        /// The rollbar offline store options
        /// </summary>
        private readonly RollbarOfflineStoreOptions _rollbarOfflineStoreOptions = 
            new RollbarOfflineStoreOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureConfig"/> class.
        /// </summary>
        public RollbarInfrastructureConfig()
            : this("seedToken")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarInfrastructureConfig"/> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        public RollbarInfrastructureConfig(string? rollbarAccessToken, string? rollbarEnvironment = null)
        {
            this._rollbarLoggerConfig = new RollbarLoggerConfig(rollbarAccessToken, rollbarEnvironment);
        }

        /// <summary>
        /// Gets the rollbar logger configuration.
        /// </summary>
        /// <value>The rollbar logger configuration.</value>
        public IRollbarLoggerConfig RollbarLoggerConfig
        {
            get
            {
                return this._rollbarLoggerConfig;
            }
        }
        /// <summary>
        /// Gets the rollbar infrastructure options.
        /// </summary>
        /// <value>The rollbar infrastructure options.</value>
        public IRollbarInfrastructureOptions RollbarInfrastructureOptions
        {
            get
            {
                return this._rollbarInfrastructureOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar telemetry options.
        /// </summary>
        /// <value>The rollbar telemetry options.</value>
        public IRollbarTelemetryOptions RollbarTelemetryOptions
        {
            get
            {
                return this._rollbarTelemetryOptions;
            }
        }

        /// <summary>
        /// Gets the rollbar offline store options.
        /// </summary>
        /// <value>The rollbar offline store options.</value>
        public IRollbarOfflineStoreOptions RollbarOfflineStoreOptions
        {
            get
            {
                return this._rollbarOfflineStoreOptions;
            }
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        public override RollbarInfrastructureConfig Reconfigure(IRollbarInfrastructureConfig likeMe)
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
                new Validator<RollbarInfrastructureConfig, RollbarInfrastructureConfig.RollbarInfrastructureConfigValidationRule>()
                    .AddValidation(
                        RollbarInfrastructureConfig.RollbarInfrastructureConfigValidationRule.LoggerConfigRequired,
                        (config) => { return this.RollbarLoggerConfig != null; }
                        )
                    .AddValidation(
                        RollbarInfrastructureConfig.RollbarInfrastructureConfigValidationRule.InfrastructureOptionsRequred,
                        (config) => { return this.RollbarInfrastructureOptions != null; }
                        )
                    .AddValidation(
                        RollbarInfrastructureConfig.RollbarInfrastructureConfigValidationRule.TelemetryOptionsRequired,
                        (config) => { return this.RollbarTelemetryOptions != null; }
                        )
                    .AddValidation(
                        RollbarInfrastructureConfig.RollbarInfrastructureConfigValidationRule.OfflineStoreOptionsRequired,
                        (config) => { return this.RollbarOfflineStoreOptions != null; }
                        )
               ;

            IValidatable[] validatableComponents =
            {
                this._rollbarLoggerConfig,
                this._rollbarInfrastructureOptions,
                this._rollbarTelemetryOptions,
                this._rollbarOfflineStoreOptions,
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
        /// Enum RollbarInfrastructureConfigValidationRule
        /// </summary>
        public enum RollbarInfrastructureConfigValidationRule
        {
            /// <summary>
            /// The logger configuration required
            /// </summary>
            LoggerConfigRequired,
            /// <summary>
            /// The infrastructure options requred
            /// </summary>
            InfrastructureOptionsRequred,
            /// <summary>
            /// The telemetry options required
            /// </summary>
            TelemetryOptionsRequired,
            /// <summary>
            /// The offline store options required
            /// </summary>
            OfflineStoreOptionsRequired,
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarInfrastructureConfig IReconfigurable<IRollbarInfrastructureConfig, IRollbarInfrastructureConfig>.Reconfigure(IRollbarInfrastructureConfig likeMe)
        {
            return this.Reconfigure(likeMe);
        }
    }
}
