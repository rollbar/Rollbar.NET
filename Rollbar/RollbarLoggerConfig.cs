namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public class RollbarLoggerConfig
        : ReconfigurableBase<RollbarLoggerConfig, IRollbarLoggerConfig>
        , IRollbarLoggerConfig
    {
        private RollbarDestinationOptions _rollbarDestinationOptions = null;
        private HttpProxyOptions _httpProxyOptions = null;
        private RollbarDeveloperOptions _rollbarDeveloperOptions = null;
        private RollbarDataSecurityOptions _rollbarDataSecurityOptions = null;
        private RollbarPayloadAdditionOptions _rollbarPayloadAdditionOptions = null;
        private RollbarPayloadManipulationOptions _rollbarPayloadManipulationOptions = null;

        public IRollbarDestinationOptions RollbarDestinationOptions
        {
            get
            {
                if(this._rollbarDestinationOptions == null)
                {
                    this._rollbarDestinationOptions = new RollbarDestinationOptions();
                }

                return this._rollbarDestinationOptions;
            }
        }

        public IHttpProxyOptions HttpProxyOptions
        {
            get
            {
                if(this._httpProxyOptions == null)
                {
                    this._httpProxyOptions = new HttpProxyOptions();
                }

                return this._httpProxyOptions;
            }
        }

        public IRollbarDeveloperOptions RollbarDeveloperOptions
        {
            get
            {
                if(this._rollbarDeveloperOptions == null)
                {
                    this._rollbarDeveloperOptions = new RollbarDeveloperOptions();
                }

                return this._rollbarDeveloperOptions;
            }
        }

        public IRollbarDataSecurityOptions RollbarDataSecurityOptions
        {
            get
            {
                if(this._rollbarDataSecurityOptions == null)
                {
                    this._rollbarDataSecurityOptions = new RollbarDataSecurityOptions();
                }

                return this._rollbarDataSecurityOptions;
            }
        }

        public IRollbarPayloadAdditionOptions RollbarPayloadAdditionOptions
        {
            get
            {
                if(this._rollbarPayloadAdditionOptions == null)
                {
                    this._rollbarPayloadAdditionOptions = new RollbarPayloadAdditionOptions();
                }

                return this._rollbarPayloadAdditionOptions;
            }
        }

        public IRollbarPayloadManipulationOptions RollbarPayloadManipulationOptions
        {
            get
            {
                if(this._rollbarPayloadManipulationOptions == null)
                {
                    this._rollbarPayloadManipulationOptions = new RollbarPayloadManipulationOptions();
                }

                return this._rollbarPayloadManipulationOptions;
            }
        }

        public IRollbarLoggerConfig Reconfigure(IRollbarLoggerConfig likeMe)
        {
            return base.Reconfigure(likeMe);
        }

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

            List<IValidatable> validatableComponents = 
                new List<IValidatable>();
            if(this._rollbarDestinationOptions != null)
            {
                validatableComponents.Add(this._rollbarDestinationOptions);
            }
            if(this._httpProxyOptions != null)
            {
                validatableComponents.Add(this._httpProxyOptions);
            }
            if(this._rollbarDeveloperOptions != null)
            {
                validatableComponents.Add(this._rollbarDeveloperOptions);
            }
            if(this._rollbarDataSecurityOptions != null)
            {
                validatableComponents.Add(this._rollbarDataSecurityOptions);
            }
            if(this._rollbarPayloadAdditionOptions != null)
            {
                validatableComponents.Add(this._rollbarPayloadAdditionOptions);
            }
            if(this._rollbarPayloadManipulationOptions != null)
            {
                validatableComponents.Add(this._rollbarPayloadManipulationOptions);
            }

            CompositeValidator compositeValidator = 
                new CompositeValidator(
                    new Validator[] { validator },
                    validatableComponents
                    );

            return validator;
        }

        public enum RollbarLoggerConfigValidationRule
        {
            DestinationOptionsRequired,
            HttpProxyOptionsRequred,
            DeveloperOptionsRequired,
            DataSecurityOptionsRequired,
            PayloadAdditionOptionsRequired,
            PayloadManipulationOptionsRequired,
        }
    }
}
