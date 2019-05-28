namespace Rollbar.NetFramework
{
    using System;
    using System.Configuration;
    using System.Diagnostics;

    /// <summary>
    /// Implements Rollbar custom configuration section for .NET Framework only!
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationSection" />
    /// <remarks>
    /// http://joelabrahamsson.com/creating-a-custom-configuration-section-in-net/
    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationsection.aspx
    /// https://docs.microsoft.com/en-us/dotnet/api/system.configuration.configurationsection?view=netframework-4.7.1
    /// </remarks>
    public class RollbarConfigSection
            : ConfigurationSection
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static RollbarConfigSection GetConfiguration()
        {
            try
            {
                RollbarConfigSection configuration =
                    ConfigurationManager.GetSection("rollbar")
                    as RollbarConfigSection;
                return configuration;
            }
            catch (Exception ex)
            {
                //let's just trace it for now:
                Trace.TraceError(
                    "Error while attempting to get RollbarConfigSection:" + System.Environment.NewLine + ex
                    );
                return null;
            }
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [ConfigurationProperty("accessToken", IsRequired = false)]
        public string AccessToken 
            => this["accessToken"] as string;

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        [ConfigurationProperty("endPoint", IsRequired = false)]
        public string EndPoint 
            => this["endPoint"] as string;

        /// <summary>
        /// Gets or sets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        [ConfigurationProperty("scrubFields", IsRequired = false)]
        public string ScrubFields 
            => this["scrubFields"] as string;

        /// <summary>
        /// Gets or sets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        [ConfigurationProperty("scrubWhitelistFields", IsRequired = false)]
        public string ScrubWhitelistFields 
            => this["scrubWhitelistFields"] as string;

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        [ConfigurationProperty("logLevel", IsRequired = false)]
        public ErrorLevel? LogLevel 
            => this["logLevel"] as ErrorLevel?;

        /// <summary>
        /// Gets or sets the enabled.
        /// </summary>
        /// <value>
        /// The enabled.
        /// </value>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool? Enabled 
            => this["enabled"] as bool?;

        /// <summary>
        /// Gets a value indicating whether this <see cref="RollbarConfigSection"/> is transmit.
        /// </summary>
        /// <value><c>null</c> if contains no value, <c>true</c> if [transmit]; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("transmit", IsRequired = false, DefaultValue = true)]
        public bool? Transmit 
            => this["transmit"] as bool?;

        /// <summary>
        /// Gets a value indicating whether to rethrow exceptions after reporting.
        /// </summary>
        /// <value><c>null</c> if contains no value, <c>true</c> if to rethrow exceptions after reporting; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("rethrowExceptionsAfterReporting", IsRequired = false, DefaultValue = false)]
        public bool? RethrowExceptionsAfterReporting 
            => this["rethrowExceptionsAfterReporting"] as bool?;

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        [ConfigurationProperty("environment", IsRequired = false)]
        public string Environment 
            => this["environment"] as string;

        /// <summary>
        /// Gets or sets the proxy address.
        /// </summary>
        /// <value>
        /// The proxy address.
        /// </value>
        [ConfigurationProperty("proxyAddress", IsRequired = false)]
        public string ProxyAddress 
            => this["proxyAddress"] as string;

        /// <summary>
        /// Gets the proxy username.
        /// </summary>
        /// <value>The proxy username.</value>
        [ConfigurationProperty("proxyUsername", IsRequired = false)]
        public string ProxyUsername 
            => this["proxyUsername"] as string;

        /// <summary>
        /// Gets the proxy password.
        /// </summary>
        /// <value>The proxy password.</value>
        [ConfigurationProperty("proxyPassword", IsRequired = false)]
        public string ProxyPassword 
            => this["proxyPassword"] as string;

        /// <summary>
        /// Gets or sets the maximum reports per minute.
        /// </summary>
        /// <value>
        /// The maximum reports per minute.
        /// </value>
        [ConfigurationProperty("maxReportsPerMinute", IsRequired = false)]
        public int? MaxReportsPerMinute 
            => this["maxReportsPerMinute"] as int?;

        /// <summary>
        /// Gets or sets the reporting queue depth.
        /// </summary>
        /// <value>
        /// The reporting queue depth.
        /// </value>
        [ConfigurationProperty("reportingQueueDepth", IsRequired = false)]
        public int? ReportingQueueDepth 
            => this["reportingQueueDepth"] as int?;

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
        [ConfigurationProperty("maxItems", IsRequired = false)]
        public int? MaxItems 
            => this["maxItems"] as int?;

        /// <summary>
        /// Gets a value indicating whether to auto-capture uncaught exceptions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if auto-capture uncaught exceptions is enabled; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("captureUncaughtExceptions", IsRequired = false)]
        public bool? CaptureUncaughtExceptions 
            => this["captureUncaughtExceptions"] as bool?;

        /// <summary>
        /// Gets the person data collection policies.
        /// </summary>
        /// <value>
        /// The person data collection policies.
        /// </value>
        [ConfigurationProperty("personDataCollectionPolicies", IsRequired = false)]
        public PersonDataCollectionPolicies? PersonDataCollectionPolicies 
            => this["personDataCollectionPolicies"] as PersonDataCollectionPolicies?;

        /// <summary>
        /// Gets the IP address collection policy.
        /// </summary>
        /// <value>
        /// The IP address collection policy.
        /// </value>
        [ConfigurationProperty("ipAddressCollectionPolicy", IsRequired = false)]
        public IpAddressCollectionPolicy? IpAddressCollectionPolicy 
            => this["ipAddressCollectionPolicy"] as IpAddressCollectionPolicy?;
    }

}
