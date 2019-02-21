namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Telemetry;

    public class ConfigAttributesPackageDecorator
        : RollbarPackageDecoratorBase
    {
        private readonly IRollbarConfig _rollbarConfig;
        private readonly DTOs.Telemetry[] _capturedTelemetryRecords;

        public ConfigAttributesPackageDecorator(
            IRollbarPackage packageToDecorate, 
            IRollbarConfig rollbarConfig
            )
            : this(packageToDecorate, rollbarConfig, false)
        {
        }

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

            //if (this._rollbarConfig.Client != null)
            //{
            //    rollbarData.Client. = this._rollbarConfig.Client;
            //}


            if (!string.IsNullOrWhiteSpace(this._rollbarConfig.Environment))
            {
                rollbarData.Environment = this._rollbarConfig.Environment;
            }

            if(this._rollbarConfig.Person != null)
            {
                rollbarData.Person = this._rollbarConfig.Person;
            }



            //try
            //{
            //    if (this._config.CheckIgnore != null
            //        && this._config.CheckIgnore.Invoke(payload)
            //        )
            //    {
            //        return;
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  check-ignoring a payload..."));
            //}

            //try
            //{
            //    this._config.Transform?.Invoke(payload);
            //}
            //catch (System.Exception ex)
            //{
            //    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  transforming a payload..."));
            //}

            //try
            //{
            //    this._config.Truncate?.Invoke(payload);
            //}
            //catch (System.Exception ex)
            //{
            //    OnRollbarEvent(new InternalErrorEventArgs(this, payload, ex, "While  truncating a payload..."));
            //}
        }
    }
}
