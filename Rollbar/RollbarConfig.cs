namespace Rollbar
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.NetFramework;
    using Rollbar.PayloadStore;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Class RollbarConfig.
    /// Implements the <see cref="Rollbar.Common.ReconfigurableBase{Rollbar.RollbarConfig, Rollbar.IRollbarConfig}" />
    /// Implements the <see cref="Rollbar.ITraceable" />
    /// Implements the <see cref="Rollbar.IRollbarConfig" />
    /// Implements the <see cref="System.IEquatable{Rollbar.IRollbarConfig}" />
    /// Implements the <see cref="Rollbar.Common.IValidatable" />
    /// </summary>
    /// <seealso cref="Rollbar.Common.ReconfigurableBase{Rollbar.RollbarConfig, Rollbar.IRollbarConfig}" />
    /// <seealso cref="Rollbar.ITraceable" />
    /// <seealso cref="Rollbar.IRollbarConfig" />
    /// <seealso cref="System.IEquatable{Rollbar.IRollbarConfig}" />
    /// <seealso cref="Rollbar.Common.IValidatable" />
    public class RollbarConfig
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
        : ReconfigurableBase<RollbarConfig, IRollbarConfig>
        , ITraceable
        , IRollbarConfig
        , IEquatable<IRollbarConfig>
        , IValidatable
    {
        private readonly RollbarLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarConfig"/> class.
        /// </summary>
        public RollbarConfig()
            : this(null as string)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarConfig"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        public RollbarConfig(string accessToken)
        {
            this.SetDefaults();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                this.AccessToken = accessToken;
            }
            else
            {
                // initialize based on application configuration file (if any):
                NetStandard.RollbarConfigUtility.Load(this);
            }
        }

        internal RollbarConfig(RollbarLogger logger)
        {
            this._logger = logger;

            this.SetDefaults();

            // initialize based on application configuration file (if any):
            NetStandard.RollbarConfigUtility.Load(this);
        }

        private void SetDefaults()
        {
            // let's set some default values:
            this.Environment = "production";
            this.Enabled = true;
            this.Transmit = true;
            this.RethrowExceptionsAfterReporting = false;
            this.EnableLocalPayloadStore = false;
            this.LocalPayloadStoreFileName = PayloadStoreConstants.DefaultRollbarStoreDbFile;
            this.LocalPayloadStoreLocationPath = PayloadStoreConstants.DefaultRollbarStoreDbFileLocation;
            this.MaxReportsPerMinute = null; //5000;
            this.ReportingQueueDepth = 20;
            this.MaxItems = 0;
            this.CaptureUncaughtExceptions = true;
            this.LogLevel = ErrorLevel.Debug;
            this.ScrubFields = RollbarDataScrubbingHelper.Instance.GetDefaultFields().ToArray();
            this.ScrubSafelistFields = new string[] { };
            this.EndPoint = "https://api.rollbar.com/api/1/";
            this.ProxyAddress = null;
            this.ProxyUsername = null;
            this.ProxyPassword = null;
            this.PayloadPostTimeout = TimeSpan.FromSeconds(30);
            this.CheckIgnore = null;
            this.Transform = null;
            this.Truncate = null;
            this.Server = null;
            this.Person = null;

            this.PersonDataCollectionPolicies = PersonDataCollectionPolicies.None;
            this.IpAddressCollectionPolicy = IpAddressCollectionPolicy.Collect;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        internal RollbarLogger Logger
        {
            get { return this._logger; }
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>
        /// Reconfigured instance.
        /// </returns>
        public override RollbarConfig Reconfigure(IRollbarConfig likeMe)
        {
            base.Reconfigure(likeMe);

            if (this.Logger != null && this.Logger.Queue != null)
            {
                var rollbarClient = new RollbarClient(this.Logger);
                // reset the queue to use the new RollbarClient:
                this.Logger.Queue.Flush();
                this.Logger.Queue.UpdateClient(rollbarClient);
            }

            return this;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public string EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        public string[] ScrubFields
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the scrub safelist fields.
        /// </summary>
        /// <value>The scrub safelist fields.</value>
        /// <remarks>
        /// The fields mentioned in this list are guaranteed to be excluded
        /// from the ScrubFields list in cases when the lists overlap.
        /// </remarks>
        public string[] ScrubSafelistFields { get; set; }

        /// <summary>
        /// Gets or sets the scrub whitelist fields.
        /// </summary>
        /// <value>The scrub whitelist fields.</value>
        /// <remarks>
        /// The fields mentioned in this list are guaranteed to be excluded
        /// from the ScrubFields list in cases when the lists overlap.
        /// </remarks>
        [Obsolete("Use the ScrubSafelistFields property instead.")]
        public string[] ScrubWhitelistFields
        {
            get { return this.ScrubSafelistFields; }
            set { this.ScrubSafelistFields = value; }
        }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        [JsonProperty("LogLevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ErrorLevel? LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the enabled.
        /// </summary>
        /// <value>
        /// The enabled.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger will actually transmit the payloads to the Rollbar API server.
        /// </summary>
        /// <value><c>null</c> if contains no value, <c>true</c> if transmit; otherwise, <c>false</c>.</value>
        /// <remarks>Should the SDK actually perform HTTP requests to Rollbar API. This is useful if you are trying to run Rollbar in dry run mode for development or tests.
        /// If this is false then we do all of the report processing except making the post request at the end of the pipeline.
        /// Default: true</remarks>
        public bool Transmit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to rethrow exceptions after reporting them to Rollbar API.
        /// </summary>
        /// <value><c>true</c> if to rethrow exceptions after reporting them to Rollbar API; otherwise, <c>false</c>.</value>
        public bool RethrowExceptionsAfterReporting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable local payload store.
        /// </summary>
        /// <value><c>true</c> if to enable local payload store; otherwise, <c>false</c>.</value>
        public bool EnableLocalPayloadStore { get; set; }

        /// <summary>
        /// Gets or sets the name of the local payload store file.
        /// </summary>
        /// <value>The name of the local payload store file.</value>
        public string LocalPayloadStoreFileName { get; set; }

        /// <summary>
        /// Gets or sets the local payload store location path.
        /// </summary>
        /// <value>The local payload store location path.</value>
        public string LocalPayloadStoreLocationPath { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the check ignore.
        /// </summary>
        /// <value>
        /// The check ignore.
        /// </value>
        [JsonIgnore]
        public Func<Payload, bool> CheckIgnore { get; set; }

        /// <summary>
        /// Gets or sets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        [JsonIgnore]
        public Action<Payload> Transform { get; set; }

        /// <summary>
        /// Gets or sets the truncate.
        /// </summary>
        /// <value>
        /// The truncate.
        /// </value>
        [JsonIgnore]
        public Action<Payload> Truncate { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public Server Server { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets the proxy address.
        /// </summary>
        /// <value>
        /// The proxy address.
        /// </value>
        public string ProxyAddress { get; set; }

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// Gets or sets the payload POST timeout.
        /// </summary>
        /// <value>The payload POST timeout.</value>
        public TimeSpan PayloadPostTimeout { get; set; }

        /// <summary>
        /// Gets or sets the maximum reports per minute.
        /// </summary>
        /// <value>
        /// The maximum reports per minute.
        /// </value>
        public int? MaxReportsPerMinute { get; set; }

        /// <summary>
        /// Gets or sets the reporting queue depth.
        /// </summary>
        /// <value>
        /// The reporting queue depth.
        /// </value>
        public int ReportingQueueDepth { get; set; }

        /// <summary>
        /// Gets or sets the maximum items limit.
        /// </summary>
        /// <value>
        /// The maximum items.
        /// </value>
        /// <remarks>
        /// Max number of items to report per page load or per web request.
        /// When this limit is reached, an additional item will be reported stating that the limit was reached.
        /// Like MaxReportsPerMinute, this limit counts uncaught errors and any direct calls to Rollbar.log/debug/info/warning/error/critical().
        /// Default: 0 (no limit)
        /// </remarks>
        public int MaxItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-capture uncaught exceptions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if auto-capture uncaught exceptions is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool CaptureUncaughtExceptions { get; set; }

        /// <summary>
        /// Gets or sets the person data collection policies.
        /// </summary>
        /// <value>
        /// The person data collection policies.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonDataCollectionPolicies PersonDataCollectionPolicies { get; set; }

        /// <summary>
        /// Gets or sets the IP address collection policy.
        /// </summary>
        /// <value>
        /// The IP address collection policy.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public IpAddressCollectionPolicy IpAddressCollectionPolicy { get; set; }

        /// <summary>
        /// Gets the full-path-name of the local payload store.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GetLocalPayloadStoreFullPathName()
        {
            string dbLocation = string.IsNullOrWhiteSpace(this.LocalPayloadStoreLocationPath)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFileLocation
                : this.LocalPayloadStoreLocationPath;

            string dbFile = string.IsNullOrWhiteSpace(this.LocalPayloadStoreFileName)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFile
                : this.LocalPayloadStoreFileName;

            string result = string.IsNullOrWhiteSpace(dbLocation)
                ? dbFile
                : Path.Combine(dbLocation, dbFile);

            return result;
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string TraceAsString()
        {
            return this.TraceAsString(string.Empty);
        }

        /// <summary>
        /// Traces as a string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public string TraceAsString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + this.GetType().Name + ":");
            sb.AppendLine(indent + "  AccessToken: " + this.AccessToken);
            sb.AppendLine(indent + "  EndPoint: " + this.EndPoint);
            sb.AppendLine(indent + "  ScrubFields: " + this.ScrubFields);
            sb.AppendLine(indent + "  ScrubSafelistFields: " + this.ScrubSafelistFields);
            sb.AppendLine(indent + "  Enabled: " + this.Enabled);
            sb.AppendLine(indent + "  Environment: " + this.Environment);
            sb.AppendLine(indent + "  Server: " + this.Server);
            sb.AppendLine(indent + "  Person: " + this.Person);
            sb.AppendLine(indent + "  ProxyAddress: " + this.ProxyAddress);
            sb.AppendLine(indent + "  ProxyPassword: " + this.ProxyPassword);
            sb.AppendLine(indent + "  PayloadPostTimeout: " + this.PayloadPostTimeout);
            sb.AppendLine(indent + "  MaxReportsPerMinute: " + this.MaxReportsPerMinute);
            sb.AppendLine(indent + "  ReportingQueueDepth: " + this.ReportingQueueDepth);
            sb.AppendLine(indent + "  MaxItems: " + this.MaxItems);
            sb.AppendLine(indent + "  CaptureUncaughtExceptions: " + this.CaptureUncaughtExceptions);
            sb.AppendLine(indent + "  IpAddressCollectionPolicy: " + this.IpAddressCollectionPolicy);
            sb.AppendLine(indent + "  PersonDataCollectionPolicies: " + this.PersonDataCollectionPolicies);
            sb.AppendLine(indent + "  StoreDb: " + this.GetLocalPayloadStoreFullPathName());

            return sb.ToString();
        }

        /// <summary>
        /// Gets the fields to scrub.
        /// </summary>
        /// <returns>
        /// Actual fields to be scrubbed based on combining the ScrubFields with the ScrubWhitelistFields.
        /// Basically this.ScrubFields "minus" this.ScrubWhitelistFields.
        /// </returns>
        public virtual IReadOnlyCollection<string> GetFieldsToScrub()
        {
            if (this.ScrubFields == null || this.ScrubFields.Length == 0)
            {
                return new string[0];
            }

            if (this.ScrubSafelistFields == null || this.ScrubSafelistFields.Length == 0)
            {
                return this.ScrubFields.ToArray();
            }

            var whitelist = this.ScrubSafelistFields.ToArray();
            return this.ScrubFields.Where(i => !whitelist.Contains(i)).ToArray();
        }

        /// <summary>
        /// Reconfigures this object similar to the specified one.
        /// </summary>
        /// <param name="likeMe">The pre-configured instance to be cloned in terms of its configuration/settings.</param>
        /// <returns>Reconfigured instance.</returns>
        IRollbarConfig IReconfigurable<IRollbarConfig, IRollbarConfig>.Reconfigure(IRollbarConfig likeMe)
        {
            return this.Reconfigure(likeMe);
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules.</returns>
        public IReadOnlyCollection<ValidationResult> Validate()
        {
            var validator = this.GetValidator();

            var failedValidations = validator.Validate(this);

            return failedValidations;
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public Validator GetValidator()
        {
            var validator = new Validator<RollbarConfig, RollbarConfig.RollbarConfigValidationRule>()
                    .AddValidation(
                        RollbarConfig.RollbarConfigValidationRule.ValidAccessTokenRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.AccessToken); }
                        )
                    .AddValidation(
                        RollbarConfig.RollbarConfigValidationRule.ValidEndPointRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.EndPoint); }
                        )
                    .AddValidation(
                        RollbarConfig.RollbarConfigValidationRule.ValidEnvironmentRequired,
                        (config) => { return !string.IsNullOrWhiteSpace(config.Environment); }
                        )
                    .AddValidation(
                        RollbarConfig.RollbarConfigValidationRule.ValidPersonIfAny,
                        (config) => config.Person,
                        this.Person?.GetValidator() as Validator<Person>
                        )
               ;

            return validator;
        }

        /// <summary>
        /// Enum RollbarConfigValidationRule
        /// </summary>
        public enum RollbarConfigValidationRule
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

            /// <summary>
            /// The valid person (if any)
            /// </summary>
            ValidPersonIfAny,
        }
    }
}
