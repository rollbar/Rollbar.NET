namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json;

    using Rollbar.Common;
    using Rollbar.NetStandard;
    using Rollbar.PayloadStore;

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
        //private RollbarInfrastructureOptions _rollbarInfrastructureOptions = null;

        private readonly IRollbar _logger;

        public RollbarLoggerConfig()
            : this(null as string)
        {
        }

        public RollbarLoggerConfig(string accessToken)
        {
            this.SetDefaults();

            if(!string.IsNullOrWhiteSpace(accessToken))
            {
                this._rollbarDestinationOptions.AccessToken = accessToken;
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
            // let's set some default values:
            this._rollbarDestinationOptions = 
                new RollbarDestinationOptions();
            this._rollbarDestinationOptions.EndPoint = "https://api.rollbar.com/api/1/";
            this._rollbarDestinationOptions.Environment = "production";

            this._rollbarDeveloperOptions = 
                new RollbarDeveloperOptions();
            this._rollbarDeveloperOptions.LogLevel = ErrorLevel.Debug;
            this._rollbarDeveloperOptions.Enabled = true;
            this._rollbarDeveloperOptions.Transmit = true;
            this._rollbarDeveloperOptions.RethrowExceptionsAfterReporting = false;

            this._httpProxyOptions = 
                new HttpProxyOptions();
            this._httpProxyOptions.ProxyAddress = null;
            this._httpProxyOptions.ProxyUsername = null;
            this._httpProxyOptions.ProxyPassword = null;

            this._rollbarPayloadManipulationOptions = 
                new RollbarPayloadManipulationOptions();
            this._rollbarPayloadManipulationOptions.CheckIgnore = null;
            this._rollbarPayloadManipulationOptions.Transform = null;
            this._rollbarPayloadManipulationOptions.Truncate = null;

            this._rollbarPayloadAdditionOptions = 
                new RollbarPayloadAdditionOptions();
            this._rollbarPayloadAdditionOptions.Server = null;
            this._rollbarPayloadAdditionOptions.Person = null;

            this._rollbarDataSecurityOptions = 
                new RollbarDataSecurityOptions();
            this._rollbarDataSecurityOptions.ScrubFields = RollbarDataScrubbingHelper.Instance.GetDefaultFields().ToArray();
            this._rollbarDataSecurityOptions.ScrubSafelistFields = new string[] { };
            this._rollbarDataSecurityOptions.PersonDataCollectionPolicies = PersonDataCollectionPolicies.None;
            this._rollbarDataSecurityOptions.IpAddressCollectionPolicy = IpAddressCollectionPolicy.Collect;

            //this._rollbarInfrastructureOptions = 
            //    new RollbarInfrastructureOptions();
            //this._rollbarInfrastructureOptions.PayloadPostTimeout = TimeSpan.FromSeconds(30);
            //this._rollbarInfrastructureOptions.MaxReportsPerMinute = null; //5000;
            //this._rollbarInfrastructureOptions.ReportingQueueDepth = 20;
            ////TODO: RollbarConfig!!!
            //this._rollbarInfrastructureOptions.CaptureUncaughtExceptions = true;
            //this._rollbarInfrastructureOptions.MaxItems = 0;
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

        [JsonIgnore]
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

        //public IRollbarInfrastructureOptions RollbarInfrastructureOptions
        //{
        //    get
        //    {
        //        if(this._rollbarInfrastructureOptions == null)
        //        {
        //            this._rollbarInfrastructureOptions = new RollbarInfrastructureOptions();
        //        }

        //        return this._rollbarInfrastructureOptions;
        //    }
        //}


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
                    //.AddValidation(
                    //    RollbarLoggerConfig.RollbarLoggerConfigValidationRule.InfrastructureOptionsRequired,
                    //    (config) => { return this.RollbarInfrastructureOptions != null; }
                    //    )
               ;

            IValidatable[] validatableComponents =
            {
                this._rollbarDestinationOptions,
                this._httpProxyOptions,
                this._rollbarDeveloperOptions,
                this._rollbarDataSecurityOptions,
                this._rollbarPayloadAdditionOptions,
                this._rollbarPayloadManipulationOptions,
                //this._rollbarInfrastructureOptions,
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
            //InfrastructureOptionsRequired,
        }

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
        //IRollbarInfrastructureOptions IRollbarLoggerConfig.RollbarInfrastructureOptions
        //{
        //    get
        //    {
        //        return this.RollbarInfrastructureOptions;
        //    }
        //}

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

    }
}
