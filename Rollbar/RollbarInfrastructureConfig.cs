namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Rollbar.Common;

    public class RollbarInfrastructureConfig
        : ReconfigurableBase<RollbarInfrastructureConfig, IRollbarInfrastructureConfig>
        , IRollbarInfrastructureConfig

    {
        private RollbarLoggerConfig _rollbarLoggerConfig = null;
        private RollbarInfrastructureOptions _rollbarInfrastructureOptions = null;

        public IRollbarLoggerConfig RollbarLoggerConfig
        {
            get
            {
                if(this._rollbarLoggerConfig == null)
                {
                    this._rollbarLoggerConfig =new RollbarLoggerConfig();
                }

                return this._rollbarLoggerConfig;
            }
        }
        public IRollbarInfrastructureOptions RollbarInfrastructureOptions
        {
            get
            {
                if(this._rollbarInfrastructureOptions == null)
                {
                    this._rollbarInfrastructureOptions = new RollbarInfrastructureOptions();
                }

                return this._rollbarInfrastructureOptions;
            }
        }

        public IRollbarInfrastructureConfig Reconfigure(IRollbarInfrastructureConfig likeMe)
        {
            return base.Reconfigure(likeMe);
        }

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
               ;

            IValidatable[] validatableComponents =
            {
                this._rollbarLoggerConfig,
                this._rollbarInfrastructureOptions,
            };

            Debug.Assert(validator.TotalValidationRules == (new HashSet<IValidatable>(validatableComponents)).Count);
            CompositeValidator compositeValidator =
                new CompositeValidator(
                    new Validator[] { validator },
                    validatableComponents
                    );

            return compositeValidator;
        }

        public enum RollbarInfrastructureConfigValidationRule
        {
            LoggerConfigRequired,
            InfrastructureOptionsRequred,
        }

    }
}
