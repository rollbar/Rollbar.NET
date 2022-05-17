namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Data DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Data
        : DtoBase
    {
        /// <summary>
        /// The default language
        /// </summary>
        internal static string DefaultLanguage { get; set; } = "c#";

        /// <summary>
        /// The default platform
        /// </summary>
        internal static string DefaultPlatform { get; set; } = RuntimeEnvironmentUtility.GetOSDescription();

        /// <summary>
        /// The default framework value
        /// </summary>
        internal static string DefaultFrameworkValue { get; set; } = Data.DetectTargetFrameworks();

        private static string DetectTargetFrameworks()
        {
            var targetFrameworks = RuntimeEnvironmentUtility.GetAssemblyTargetFrameworks(typeof(Data));
            return StringUtility.Combine(targetFrameworks, "; ");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="custom">The custom.</param>
        public Data(
            Body body,
            IDictionary<string, object?>? custom
            )
            : this(null, body, custom, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        public Data(
            Body body
            )
            : this(null, body)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="body">The body.</param>
        public Data(
            IRollbarLoggerConfig? config,
            Body body
            )
            : this(config, body, null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="body">The body.</param>
        /// <param name="request">The request.</param>
        public Data(
            IRollbarLoggerConfig? config,
            Body body,
            Request request
            )
            : this(config, body, null, request)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="body">The body.</param>
        /// <param name="custom">The custom.</param>
        public Data(
            IRollbarLoggerConfig? config,
            Body body,
            IDictionary<string, object?>? custom
            )
            : this(config, body, custom, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="body">The body.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="request">The request.</param>
        public Data(
            IRollbarLoggerConfig? config, 
            Body body, 
            IDictionary<string, object?>? custom, 
            Request? request
            )
        {
            Assumption.AssertNotNull(body, nameof(body));

            // snap config values:
            if (config != null)
            {
                this.Environment = config.RollbarDestinationOptions.Environment;
                this.Level = config.RollbarDeveloperOptions.LogLevel;
                this.Person = config.RollbarPayloadAdditionOptions.Person;
                this.Server = config.RollbarPayloadAdditionOptions.Server;
                this.CodeVersion = config.RollbarPayloadAdditionOptions.CodeVersion;
            }

            // set explicit values:
            this.Body = body;
            this.Request = request;
            this.Custom = custom;

            // set calculated values:
            this.Platform = Data.DefaultPlatform;
            this.Framework = Data.DefaultFrameworkValue;
            this.Language = Data.DefaultLanguage;
            this.Notifier = new Notifier();
            this.GuidUuid = Guid.NewGuid();
            this.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the environment (REQUIRED).
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        /// <remarks>
        /// Required: environment
        /// The name of the environment in which this occurrence was seen.
        /// A string up to 255 characters. For best results, use "production" or "prod" for your
        /// production environment.
        /// You don't need to configure anything in the Rollbar UI for new environment names;
        /// we'll detect them automatically.
        /// </remarks>
        [JsonProperty("environment", Required = Required.Always)]
        public string? Environment { get; set; }

        /// <summary>
        /// Gets the body (REQUIRED).
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        /// <remarks>
        /// Required: body
        /// The main data being sent. 
        /// It can either be a message, an exception, or a crash report.
        /// </remarks>
        [JsonProperty("body", Required = Required.Always)]
        public Body Body { get; private set; }

        /// <summary>
        /// Gets or sets the level (OPTIONAL).
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        /// <remarks>
        /// Optional: level
        /// The severity level. One of: "critical", "error", "warning", "info", "debug"
        /// Defaults to "error" for exceptions and "info" for messages.
        /// The level of the *first* occurrence of an item is used as the item's level.
        /// </remarks>
        [JsonProperty("level", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ErrorLevel? Level { get; set; }

        /// <summary>
        /// Gets or sets the timestamp (OPTIONAL).
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        /// <remarks>
        /// Optional: timestamp
        /// When this occurred, as a unix timestamp.
        /// </remarks>
        [JsonProperty("timestamp", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the code version (OPTIONAL).
        /// </summary>
        /// <value>
        /// The code version.
        /// </value>
        /// <remarks>
        /// Optional: code_version
        /// A string, up to 40 characters, describing the version of the application code
        /// Rollbar understands these formats:
        /// - semantic version (i.e. "2.1.12")
        /// - integer (i.e. "45")
        /// - git SHA (i.e. "3da541559918a808c2402bba5012f6c60b27661c")
        /// If you have multiple code versions that are relevant, those can be sent inside "client" and "server"
        /// (see those sections below)
        /// For most cases, just send it here.
        /// </remarks>
        [JsonProperty("code_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? CodeVersion { get; set; }

        /// <summary>
        /// Gets or sets the platform (OPTIONAL).
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        /// <remarks>
        /// Optional: platform
        /// The platform on which this occurred. Meaningful platform names:
        /// "browser", "android", "ios", "flash", "client", "heroku", "google-app-engine"
        /// If this is a client-side event, be sure to specify the platform and use a post_client_item access token.
        /// </remarks>
        [JsonProperty("platform", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Platform { get; set; }

        /// <summary>
        /// Gets or sets the language (OPTIONAL).
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        /// <remarks>
        /// Optional: language
        /// The name of the language your code is written in.
        /// This can affect the order of the frames in the stack trace. The following languages set the most
        /// recent call first - 'ruby', 'javascript', 'php', 'java', 'objective-c', 'lua'
        /// It will also change the way the individual frames are displayed, with what is most consistent with
        /// users of the language.
        /// </remarks>
        [JsonProperty("language", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Language { get; set; }

        /// <summary>
        /// Gets or sets the framework (OPTIONAL).
        /// </summary>
        /// <value>
        /// The framework.
        /// </value>
        /// <remarks>
        /// Optional: framework
        /// The name of the framework your code uses
        /// </remarks>
        [JsonProperty("framework", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Framework { get; set; }

        /// <summary>
        /// Gets or sets the context (OPTIONAL).
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        /// <remarks>
        /// Optional: context
        /// An identifier for which part of your application this event came from.
        /// Items can be searched by context (prefix search)
        /// For example, in a Rails app, this could be `controller#action`.
        /// In a single-page javascript app, it could be the name of the current screen or route.
        /// </remarks>
        [JsonProperty("context", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Context { get; set; }

        /// <summary>
        /// Gets or sets the request (OPTIONAL).
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        /// <remarks>
        /// Optional: request
        /// Data about the request this event occurred in.
        /// </remarks>
        [JsonProperty("request", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Request? Request { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>The response.</value>
        /// <remarks>
        /// Optional: response
        /// Data about the relevant HTTP response (if any).
        /// </remarks>
        [JsonProperty("response", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Response? Response { get; set; }

        /// <summary>
        /// Gets or sets the person (OPTIONAL).
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        /// <remarks>
        /// Optional: person
        /// The user affected by this event. Will be indexed by ID, username, and email.
        /// People are stored in Rollbar keyed by ID. If you send a multiple different usernames/emails for the
        /// same ID, the last received values will overwrite earlier ones.
        /// </remarks>
        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Person? Person { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        /// <remarks>
        /// Optional: server
        /// Data about the server related to this event.
        /// </remarks>
        [JsonProperty("server", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Server? Server { get; set; }

        /// <summary>
        /// Gets or sets the client (OPTIONAL).
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        /// <remarks>
        /// Optional: client
        /// Data about the client device this event occurred on.
        /// As there can be multiple client environments for a given event (i.e. Flash running inside
        /// an HTML page), data should be namespaced by platform.
        /// </remarks>
        [JsonProperty("client", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Client? Client { get; set; }

        /// <summary>
        /// Gets or sets the custom (OPTIONAL).
        /// </summary>
        /// <value>
        /// The custom.
        /// </value>
        /// <remarks>
        /// Optional: custom
        /// Any arbitrary metadata you want to send. "custom" itself should be an object.
        /// </remarks>
        [JsonProperty("custom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, object?>? Custom { get; set; }

        /// <summary>
        /// Gets or sets the fingerprint (OPTIONAL).
        /// </summary>
        /// <value>
        /// The fingerprint.
        /// </value>
        /// <remarks>
        /// Optional: fingerprint
        /// A string controlling how this occurrence should be grouped. Occurrences with the same
        /// fingerprint are grouped together. See the "Grouping" guide for more information.
        /// Should be a string up to 40 characters long; if longer than 40 characters, we'll use its SHA1 hash.
        /// If omitted, we'll determine this on the backend.
        /// </remarks>
        [JsonProperty("fingerprint", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets the title (OPTIONAL).
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        /// <remarks>
        /// Optional: title
        /// A string that will be used as the title of the Item occurrences will be grouped into.
        /// Max length 255 characters.
        /// If omitted, we'll determine this on the backend.
        /// </remarks>
        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the UUID (OPTIONAL).
        /// </summary>
        /// <value>
        /// The UUID.
        /// </value>
        /// <remarks>
        /// Optional: uuid
        /// A string, up to 36 characters, that uniquely identifies this occurrence.
        /// While it can now be any latin1 string, this may change to be a 16 byte field in the future.
        /// We recommend using a UUID4 (16 random bytes).
        /// The UUID space is unique to each project, and can be used to look up an occurrence later.
        /// It is also used to detect duplicate requests. If you send the same UUID in two payloads, the second
        /// one will be discarded.
        /// While optional, it is recommended that all clients generate and provide this field
        /// </remarks>
        [JsonProperty("uuid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Uuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier UUID (OPTIONAL).
        /// </summary>
        /// <value>
        /// The unique identifier UUID.
        /// </value>
        /// <remarks>
        /// Optional: Guid-like uuid ("mimics" the Uuid property above)
        /// A string, up to 36 characters, that uniquely identifies this occurrence.
        /// While it can now be any latin1 string, this may change to be a 16 byte field in the future.
        /// We recommend using a UUID4 (16 random bytes).
        /// The UUID space is unique to each project, and can be used to look up an occurrence later.
        /// It is also used to detect duplicate requests. If you send the same UUID in two payloads, the second
        /// one will be discarded.
        /// While optional, it is recommended that all clients generate and provide this field
        /// </remarks>
        [JsonIgnore]
        public Guid? GuidUuid
        {
            get { return Uuid == null ? (Guid?)null : Guid.Parse(Uuid); }
            private set { Uuid = value?.ToString("N"); }
        }

        /// <summary>
        /// Gets the notifier (OPTIONAL).
        /// </summary>
        /// <value>
        /// The notifier.
        /// </value>
        /// <remarks>
        /// Optional: notifier
        /// Describes the library used to send this event.
        /// </remarks>
        [JsonProperty("notifier", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Notifier Notifier
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        public override Validator GetValidator()
        {
            var validator = new Validator<Data, Data.DataValidationRule>()
                    .AddValidation(
                        Data.DataValidationRule.EnvironmentRequired,
                        (data) => { return !string.IsNullOrWhiteSpace(data.Environment); }
                        )
                    .AddValidation(
                        Data.DataValidationRule.BodyRequired,
                        (data) => { return (data.Body != null); }
                        )
                    .AddValidation(
                        Data.DataValidationRule.ValidBody,
                        (data) => data.Body,
                        this.Body?.GetValidator() as Validator<Body>
                        )
                    .AddValidation(
                        Data.DataValidationRule.ValidClientIfAny,
                        (data) => data.Client,
                        this.Client?.GetValidator() as Validator<Client?>
                        )
                    .AddValidation(
                        Data.DataValidationRule.ValidPersonIfAny,
                        (data) => data.Person,
                        this.Person?.GetValidator() as Validator<Person?>
                        )
                    .AddValidation(
                        Data.DataValidationRule.ValidRequestIfAny,
                        (data) => data.Request,
                        this.Request?.GetValidator() as Validator<Request?>
                        )
                    .AddValidation(
                        Data.DataValidationRule.ValidServerIfAny,
                        (data) => data.Server,
                        this.Server?.GetValidator() as Validator<Server?>
                        )
                    .AddValidation(
                        Data.DataValidationRule.NotifierRequired,
                        (data) => { return (data.Notifier != null); }
                    )
               ;

            return validator;
        }

        /// <summary>
        /// Enum DataValidationRule
        /// </summary>
        public enum DataValidationRule
        {
            /// <summary>
            /// The environment required
            /// </summary>
            EnvironmentRequired,

            /// <summary>
            /// The body required
            /// </summary>
            BodyRequired,

            /// <summary>
            /// The valid body
            /// </summary>
            ValidBody,

            /// <summary>
            /// The valid server if any
            /// </summary>
            ValidServerIfAny,

            /// <summary>
            /// The valid request if any
            /// </summary>
            ValidRequestIfAny,

            /// <summary>
            /// The valid person if any
            /// </summary>
            ValidPersonIfAny,

            /// <summary>
            /// The valid client if any
            /// </summary>
            ValidClientIfAny,

            /// <summary>
            /// The notifier required
            /// </summary>
            NotifierRequired,
        }
    }
}
