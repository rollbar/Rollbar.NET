namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;

    public class RollbarDestinationOptions
        : ReconfigurableBase<RollbarDestinationOptions, IRollbarDestinationOptions>
        , IRollbarDestinationOptions
    {
        private const string defaultEndpoint = "https://api.rollbar.com/api/1/";
        private const string defaultEnvironment = "production";

        internal RollbarDestinationOptions()
            : this(null)
        {
        }

        public RollbarDestinationOptions(string accessToken)
            : this(accessToken, RollbarDestinationOptions.defaultEnvironment)
        {
        }

        public RollbarDestinationOptions(string accessToken, string environment)
            : this(accessToken, environment, RollbarDestinationOptions.defaultEndpoint)
        {
        }

        public RollbarDestinationOptions(string accessToken, string environment, string endPoint)
        {
            this.AccessToken = accessToken;
            this.Environment = environment;
            this.EndPoint = endPoint;
        }

        public string AccessToken
        {
            get;
            set;
        }

        public string Environment
        {
            get;
            set;
        }

        public string EndPoint
        {
            get;
            set;
        }

        public IRollbarDestinationOptions Reconfigure(IRollbarDestinationOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            var validator = 
                new Validator<RollbarDestinationOptions, RollbarDestinationOptions.RollbarDestinationOptionsValidationRule>()
                    .AddValidation(
                        RollbarDestinationOptions.RollbarDestinationOptionsValidationRule.ValidAccessTokenRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.AccessToken); }
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

    }
}
