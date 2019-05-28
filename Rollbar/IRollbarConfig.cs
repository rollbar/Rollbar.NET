namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Models Rollbar configuration interface.
    /// </summary>
    public interface IRollbarConfig
        : IReconfigurable<IRollbarConfig, IRollbarConfig>
        , IEquatable<IRollbarConfig>
        , ITraceable
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        string AccessToken { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        string Environment { get; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        string EndPoint { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger, configured with this <see cref="IRollbarConfig"/>, is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Default: true
        /// </remarks>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Rollbar logger will actually transmit the payloads to the Rollbar API server.
        /// </summary>
        /// <value><c>true</c> if transmit; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Should the SDK actually perform HTTP requests to Rollbar API. This is useful if you are trying to run Rollbar in dry run mode for development or tests.
        /// If this is false then we do all of the report processing except making the post request at the end of the pipeline.
        /// Default: true
        /// </remarks>
        bool Transmit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to rethrow exceptions after reporting them to Rollbar API.
        /// </summary>
        /// <value><c>true</c> if to rethrow exceptions after reporting them to Rollbar API; otherwise, <c>false</c>.</value>
        bool RethrowExceptionsAfterReporting { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        ErrorLevel? LogLevel { get; set; }

        /// <summary>
        /// Gets the proxy address.
        /// </summary>
        /// <value>
        /// The proxy address.
        /// </value>
        string ProxyAddress { get; }

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        string ProxyUsername { get; }

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        string ProxyPassword { get; }

        /// <summary>
        /// Gets the maximum reports per minute.
        /// </summary>
        /// <value>
        /// The maximum reports per minute.
        /// </value>
        int? MaxReportsPerMinute { get; }

        /// <summary>
        /// Gets the reporting queue depth.
        /// </summary>
        /// <value>
        /// The reporting queue depth.
        /// </value>
        int ReportingQueueDepth { get; }

        /// <summary>
        /// Gets the maximum items limit.
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
        int MaxItems { get; }

        /// <summary>
        /// Gets a value indicating whether to auto-capture uncaught exceptions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if auto-capture uncaught exceptions is enabled; otherwise, <c>false</c>.
        /// </value>
        bool CaptureUncaughtExceptions { get; }

        /// <summary>
        /// Gets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        string[] ScrubFields { get; }

        /// <summary>
        /// Gets the scrub white-list fields.
        /// </summary>
        /// <value>
        /// The scrub white-list fields.
        /// </value>
        /// <remarks>
        /// The fields mentioned in this list are guaranteed to be excluded 
        /// from the ScrubFields list in cases when the lists overlap.
        /// </remarks>
        string[] ScrubWhitelistFields { get; }

        /// <summary>
        /// Gets the fields to scrub.
        /// </summary>
        /// <returns>
        /// Actual fields to be scrubbed based on combining the ScrubFields with the ScrubWhitelistFields.
        /// </returns>
        IReadOnlyCollection<string> GetFieldsToScrub();

        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <value>
        /// The transform.
        /// </value>
        Action<Payload> Transform { get; }

        /// <summary>
        /// Gets the truncate.
        /// </summary>
        /// <value>
        /// The truncate.
        /// </value>
        Action<Payload> Truncate { get; }

        /// <summary>
        /// Gets the check ignore.
        /// </summary>
        /// <value>
        /// The check ignore.
        /// </value>
        Func<Payload, bool> CheckIgnore { get; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        Person Person { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        Server Server { get; set; }

        /// <summary>
        /// Gets or sets the person data collection policies.
        /// </summary>
        /// <value>
        /// The person data collection policies.
        /// </value>
        PersonDataCollectionPolicies PersonDataCollectionPolicies { get; set; }

        /// <summary>
        /// Gets or sets the IP address collection policy.
        /// </summary>
        /// <value>
        /// The IP address collection policy.
        /// </value>
        IpAddressCollectionPolicy IpAddressCollectionPolicy { get; set; }
    }
}