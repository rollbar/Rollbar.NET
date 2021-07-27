namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Newtonsoft.Json;

    using Rollbar.Common;
    using Rollbar.NetStandard;

    public class RollbarLoggerConfig
        : ReconfigurableBase<RollbarLoggerConfig, IRollbarLoggerConfig>
        , IRollbarLoggerConfig
    {
        private readonly RollbarDestinationOptions _rollbarDestinationOptions = 
            new RollbarDestinationOptions();
        private readonly HttpProxyOptions _httpProxyOptions = 
            new HttpProxyOptions();
        private readonly RollbarDeveloperOptions _rollbarDeveloperOptions = 
            new RollbarDeveloperOptions();
        private readonly RollbarDataSecurityOptions _rollbarDataSecurityOptions = 
            new RollbarDataSecurityOptions();
        private readonly RollbarPayloadAdditionOptions _rollbarPayloadAdditionOptions = 
            new RollbarPayloadAdditionOptions();
        private readonly RollbarPayloadManipulationOptions _rollbarPayloadManipulationOptions = 
            new RollbarPayloadManipulationOptions();

        private readonly IRollbar _logger;

        public RollbarLoggerConfig()
            : this(null as string)
        {
        }

        public RollbarLoggerConfig(string accessToken, string? rollbarEnvironment = null)
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

        private void SetDefaults()
        {
            this._rollbarDestinationOptions.EndPoint = "https://api.rollbar.com/api/1/";
            this._rollbarDestinationOptions.Environment = "production";

            this._rollbarDeveloperOptions.LogLevel = ErrorLevel.Debug;
            this._rollbarDeveloperOptions.Enabled = true;
            this._rollbarDeveloperOptions.Transmit = true;
            this._rollbarDeveloperOptions.RethrowExceptionsAfterReporting = false;

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

        internal IRollbar Logger
        {
            get
            {
                return this._logger;
            }
        }

        public override RollbarLoggerConfig Reconfigure(IRollbarLoggerConfig likeMe)
        {
            base.Reconfigure(likeMe);

            RollbarLogger rollbarLogger = this.Logger as RollbarLogger;

            if(rollbarLogger != null && rollbarLogger.Queue != null)
            {
                var rollbarClient = new RollbarClient(this.Logger);
                // reset the queue to use the new RollbarClient:
                rollbarLogger.Queue.Flush();
                rollbarLogger.Queue.UpdateClient(rollbarClient);
            }

            return this;
        }

        public IRollbarDestinationOptions RollbarDestinationOptions
        {
            get
            {
                return this._rollbarDestinationOptions;
            }
        }

        public IHttpProxyOptions HttpProxyOptions
        {
            get
            {
                return this._httpProxyOptions;
            }
        }

        public IRollbarDeveloperOptions RollbarDeveloperOptions
        {
            get
            {
                return this._rollbarDeveloperOptions;
            }
        }

        public IRollbarDataSecurityOptions RollbarDataSecurityOptions
        {
            get
            {
                return this._rollbarDataSecurityOptions;
            }
        }

        public IRollbarPayloadAdditionOptions RollbarPayloadAdditionOptions
        {
            get
            {
                return this._rollbarPayloadAdditionOptions;
            }
        }

        [JsonIgnore]
        public IRollbarPayloadManipulationOptions RollbarPayloadManipulationOptions
        {
            get
            {
                return this._rollbarPayloadManipulationOptions;
            }
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

        public enum RollbarLoggerConfigValidationRule
        {
            DestinationOptionsRequired,
            HttpProxyOptionsRequred,
            DeveloperOptionsRequired,
            DataSecurityOptionsRequired,
            PayloadAdditionOptionsRequired,
            PayloadManipulationOptionsRequired,
        }

        #region IRollbarLoggerConfig

        IRollbarDestinationOptions IRollbarLoggerConfig.RollbarDestinationOptions
        {
            get
            {
                return this.RollbarDestinationOptions;
            }
        }
        IHttpProxyOptions IRollbarLoggerConfig.HttpProxyOptions
        {
            get
            {
                return this.HttpProxyOptions;
            }
        }
        IRollbarDeveloperOptions IRollbarLoggerConfig.RollbarDeveloperOptions
        {
            get
            {
                return this.RollbarDeveloperOptions;
            }
        }
        IRollbarDataSecurityOptions IRollbarLoggerConfig.RollbarDataSecurityOptions
        {
            get
            {
                return this.RollbarDataSecurityOptions;
            }
        }
        IRollbarPayloadAdditionOptions IRollbarLoggerConfig.RollbarPayloadAdditionOptions
        {
            get
            {
                return this.RollbarPayloadAdditionOptions;
            }
        }
        IRollbarPayloadManipulationOptions IRollbarLoggerConfig.RollbarPayloadManipulationOptions
        {
            get
            {
                return this.RollbarPayloadManipulationOptions;
            }
        }

        IRollbarLoggerConfig IReconfigurable<IRollbarLoggerConfig, IRollbarLoggerConfig>.Reconfigure(IRollbarLoggerConfig likeMe)
        {
            return this.Reconfigure(likeMe);
        }

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
