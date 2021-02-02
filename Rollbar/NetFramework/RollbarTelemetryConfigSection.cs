//namespace Rollbar.NetFramework
//{
//    using System;
//    using System.Configuration;
//    using System.Diagnostics;
//    using Rollbar.DTOs;

//    /// <summary>
//    /// Implements Rollbar Telemetry custom configuration section for .NET Framework only!
//    /// </summary>
//    /// <seealso cref="System.Configuration.ConfigurationSection" />
//    /// <remarks>
//    /// http://joelabrahamsson.com/creating-a-custom-configuration-section-in-net/
//    /// https://msdn.microsoft.com/en-us/library/system.configuration.configurationsection.aspx
//    /// https://docs.microsoft.com/en-us/dotnet/api/system.configuration.configurationsection?view=netframework-4.7.1
//    /// </remarks>
//    public class RollbarTelemetryConfigSection
//            : ConfigurationSection
//    {
//        private static readonly TraceSource traceSource = new TraceSource(typeof(RollbarTelemetryConfigSection).FullName);

//        /// <summary>
//        /// Gets the configuration.
//        /// </summary>
//        /// <returns></returns>
//        public static RollbarTelemetryConfigSection GetConfiguration()
//        {
//            try
//            {
//                RollbarTelemetryConfigSection configuration =
//                    ConfigurationManager.GetSection("rollbarTelemetry")
//                    as RollbarTelemetryConfigSection;
//                return configuration;
//            }
//            catch (System.Exception ex)
//            {
//                //let's just trace it for now:
//                System.Diagnostics.Trace.TraceError(
//                    "Error while attempting to get RollbarTelemetryConfigSection:" + System.Environment.NewLine + ex
//                    );
//                traceSource.TraceEvent(TraceEventType.Warning, 0, $"Error while attempting to get RollbarTelemetryConfigSection:{Environment.NewLine}{ex}");
//                return null;
//            }
//        }

//        /// <summary>
//        /// Gets the telemetry enabled.
//        /// </summary>
//        /// <value>
//        /// The telemetry enabled.
//        /// </value>
//        [ConfigurationProperty("telemetryEnabled", IsRequired = false)]
//        public bool? TelemetryEnabled
//            => this["telemetryEnabled"] as bool?;

//        /// <summary>
//        /// Gets the telemetry queue depth.
//        /// </summary>
//        /// <value>
//        /// The telemetry queue depth.
//        /// </value>
//        [ConfigurationProperty("telemetryQueueDepth", IsRequired = false)]
//        public int? TelemetryQueueDepth
//            => this["telemetryQueueDepth"] as int?;

//        /// <summary>
//        /// Gets the telemetry automatic collection types.
//        /// </summary>
//        /// <value>
//        /// The telemetry automatic collection types.
//        /// </value>
//        [ConfigurationProperty("telemetryAutoCollectionTypes", IsRequired = false)]
//        public TelemetryType? TelemetryAutoCollectionTypes
//            => this["telemetryAutoCollectionTypes"] as TelemetryType?;

//        /// <summary>
//        /// Gets the telemetry automatic collection interval.
//        /// </summary>
//        /// <value>
//        /// The telemetry automatic collection interval.
//        /// </value>
//        [ConfigurationProperty("telemetryAutoCollectionInterval", IsRequired = false)]
//        public TimeSpan? TelemetryAutoCollectionInterval
//            => this["telemetryAutoCollectionInterval"] as TimeSpan?;

//    }
//}
