namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Telemetry;

    /// <summary>
    /// Class ConfigAttributesPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class ConfigAttributesPackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The rollbar configuration
        /// </summary>
        private readonly IRollbarConfig _rollbarConfig;
        /// <summary>
        /// The captured telemetry records
        /// </summary>
        private readonly DTOs.Telemetry[] _capturedTelemetryRecords;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigAttributesPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        public ConfigAttributesPackageDecorator(
            IRollbarPackage packageToDecorate, 
            IRollbarConfig rollbarConfig
            )
            : this(packageToDecorate, rollbarConfig, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigAttributesPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public ConfigAttributesPackageDecorator(
            IRollbarPackage packageToDecorate, 
            IRollbarConfig rollbarConfig, 
            bool mustApplySynchronously
            ) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            Assumption.AssertNotNull(rollbarConfig, nameof(rollbarConfig));

            this._rollbarConfig = rollbarConfig;

            // telemetry data is based on the configuration,
            // so let's include it if applicable:
            if (TelemetryCollector.Instance.Config.TelemetryEnabled)
            {
                this._capturedTelemetryRecords =
                    TelemetryCollector.Instance.GetQueueContent();
            }
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data rollbarData)
        {
            // telemetry data is based on the configuration,
            // so let's include it if applicable:
            if (this._capturedTelemetryRecords != null)
            {
                rollbarData.Body.Telemetry = this._capturedTelemetryRecords;
            }

            if (this._rollbarConfig.Server != null)
            {
                rollbarData.Server = this._rollbarConfig.Server;
            }

            if (!string.IsNullOrWhiteSpace(this._rollbarConfig.Environment))
            {
                rollbarData.Environment = this._rollbarConfig.Environment;
            }

            if(this._rollbarConfig.Person != null)
            {
                rollbarData.Person = this._rollbarConfig.Person;
            }
        }
    }
}
