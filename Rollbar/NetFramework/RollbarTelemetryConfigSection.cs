namespace Rollbar.NetFramework
{

#if NETFX || NETSTANDARD
    using System;
    using System.Configuration;
    using Rollbar.DTOs;

    /// <summary>
    /// Implements Rollbar Telemetry custom configuration section for .NET Framework only!
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationSection" />
    /// <remarks>
    /// http://joelabrahamsson.com/creating-a-custom-configuration-section-in-net/
    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationsection.aspx
    /// https://docs.microsoft.com/en-us/dotnet/api/system.configuration.configurationsection?view=netframework-4.7.1
    /// </remarks>
    public class RollbarTelemetryConfigSection
            : ConfigurationSection
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns></returns>
        public static RollbarTelemetryConfigSection GetConfiguration()
        {
            RollbarTelemetryConfigSection configuration =
                ConfigurationManager.GetSection("rollbarTelemetry") as RollbarTelemetryConfigSection;

            if (configuration != null)
                return configuration;

            return new RollbarTelemetryConfigSection();
        }

        /// <summary>
        /// Gets the telemetry enabled.
        /// </summary>
        /// <value>
        /// The telemetry enabled.
        /// </value>
        [ConfigurationProperty("telemetryEnabled", IsRequired = false)]
        public bool? TelemetryEnabled
        {
            get { return this["telemetryEnabled"] as bool?; }
        }

        /// <summary>
        /// Gets the telemetry queue depth.
        /// </summary>
        /// <value>
        /// The telemetry queue depth.
        /// </value>
        [ConfigurationProperty("telemetryQueueDepth", IsRequired = false)]
        public int? TelemetryQueueDepth
        {
            get
            {
                return this["telemetryQueueDepth"] as int?;
            }
        }

        /// <summary>
        /// Gets the telemetry automatic collection types.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection types.
        /// </value>
        [ConfigurationProperty("telemetryAutoCollectionTypes", IsRequired = false)]
        public TelemetryType? TelemetryAutoCollectionTypes
        {
            get
            {
                return this["telemetryAutoCollectionTypes"] as TelemetryType?;
            }
        }

        /// <summary>
        /// Gets the telemetry automatic collection interval.
        /// </summary>
        /// <value>
        /// The telemetry automatic collection interval.
        /// </value>
        [ConfigurationProperty("telemetryAutoCollectionInterval", IsRequired = false)]
        public TimeSpan? TelemetryAutoCollectionInterval
        {
            get
            {
                return this["telemetryAutoCollectionInterval"] as TimeSpan?;
            }
        }

    }
#endif

}
