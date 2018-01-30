namespace Rollbar.NetFramework
{
#if NETFX_MAX_47
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Configuration;
    using System.Diagnostics;

    /// <summary>
    /// Implements Rollbar custom configuration section for .NET4.5-4.7 only!
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationSection" />
    /// <remarks>
    /// http://joelabrahamsson.com/creating-a-custom-configuration-section-in-net/
    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationsection.aspx
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
        public string[] ScrubFields
        {
            get
            {
                string scrubFields = this["scrubFields"] as string;
                if (string.IsNullOrWhiteSpace(scrubFields))
                {
                    return new string[0];
                }

                return 
                    scrubFields.Split(new string[] { ", ", "; " }, StringSplitOptions.RemoveEmptyEntries);
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
                string logLevelString = this["logLevel"] as string;
                if (string.IsNullOrWhiteSpace(logLevelString))
                {
                    return null;
                }

                ErrorLevel logLevel;
                if (Enum.TryParse<ErrorLevel>(logLevelString, out logLevel))
                {
                    return logLevel;
                }
                return null;
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
                string stringValue = this["enabled"] as string;
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                bool enabled;
                if (bool.TryParse(stringValue, out enabled))
                {
                    return enabled;
                }
                return null;
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
                return ReadOptionalInt("maxReportsPerMinute");
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
                return ReadOptionalInt("reportingQueueDepth");
            }
        }

        private int? ReadOptionalInt(string fieldName)
        {
            string stringValue = this[fieldName] as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            int intValue;
            if (int.TryParse(stringValue, out intValue))
            {
                return intValue;
            }
            return null;

        }
    }
#endif

#if NETFX_MIN_471

#endif

}
