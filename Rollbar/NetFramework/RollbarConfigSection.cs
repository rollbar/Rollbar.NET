namespace Rollbar.NetFramework
{
#if NETFX
    using System;
    using System.Configuration;

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
            RollbarConfigSection configuration =
                ConfigurationManager.GetSection("rollbar") as RollbarConfigSection;

            if (configuration != null)
                return configuration;

            return new RollbarConfigSection();
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [ConfigurationProperty("accessToken", IsRequired = false)]
        public string AccessToken
        {
            get { return this["accessToken"] as string; }
        }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        [ConfigurationProperty("endPoint", IsRequired = false)]
        public string EndPoint
        {
            get { return this["endPoint"] as string; }
        }

        /// <summary>
        /// Gets or sets the scrub fields.
        /// </summary>
        /// <value>
        /// The scrub fields.
        /// </value>
        [ConfigurationProperty("scrubFields", IsRequired = false)]
        public string ScrubFields
        {
            get
            {
                return this["scrubFields"] as string;
            }
        }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        [ConfigurationProperty("logLevel", IsRequired = false)]
        public ErrorLevel? LogLevel
        {
            get
            {
                return this["logLevel"] as ErrorLevel?;
            }
        }

        /// <summary>
        /// Gets or sets the enabled.
        /// </summary>
        /// <value>
        /// The enabled.
        /// </value>
        [ConfigurationProperty("enabled", IsRequired = false)]
        public bool? Enabled
        {
            get
            {
                return this["enabled"] as bool?;
            }
        }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        [ConfigurationProperty("environment", IsRequired = false)]
        public string Environment
        {
            get { return this["environment"] as string; }
        }

        /// <summary>
        /// Gets or sets the proxy address.
        /// </summary>
        /// <value>
        /// The proxy address.
        /// </value>
        [ConfigurationProperty("proxyAddress", IsRequired = false)]
        public string ProxyAddress
        {
            get { return this["proxyAddress"] as string; }
        }

        /// <summary>
        /// Gets or sets the maximum reports per minute.
        /// </summary>
        /// <value>
        /// The maximum reports per minute.
        /// </value>
        [ConfigurationProperty("maxReportsPerMinute", IsRequired = false)]
        public int? MaxReportsPerMinute
        {
            get
            {
                return this["maxReportsPerMinute"] as int?;
            }
        }

        /// <summary>
        /// Gets or sets the reporting queue depth.
        /// </summary>
        /// <value>
        /// The reporting queue depth.
        /// </value>
        [ConfigurationProperty("reportingQueueDepth", IsRequired = false)]
        public int? ReportingQueueDepth
        {
            get
            {
                return this["reportingQueueDepth"] as int?;
            }
        }

        [ConfigurationProperty("personDataCollectionPolicies", IsRequired = false)]
        public PersonDataCollectionPolicies? PersonDataCollectionPolicies
        {
            get
            {
                return this["personDataCollectionPolicies"] as PersonDataCollectionPolicies?;
            }
        }

        [ConfigurationProperty("ipAddressCollectionPolicy", IsRequired = false)]
        public IpAddressCollectionPolicy? IpAddressCollectionPolicy
        {
            get
            {
                return this["ipAddressCollectionPolicy"] as IpAddressCollectionPolicy?;
            }
        }
    }
#endif


}
