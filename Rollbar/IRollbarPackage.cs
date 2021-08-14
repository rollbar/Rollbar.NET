namespace Rollbar
{
    using Rollbar.DTOs;

    /// <summary>
    /// Interface IRollbarPackage
    /// 
    /// Defines API for implementing packaging strategies of how to package 
    /// a particular object type as Rollbar Data DTO.
    /// An instance implementing this interface "wraps" around the object to package 
    /// and can be passed to most of the logging methods instead of the "raw" object.  
    /// Rollbar Logger, then, would use this strategy to package the object as 
    /// a Rollbar Data DTO according to the strategy implementation.
    /// </summary>
    public interface IRollbarPackage
    {
        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        Data? PackageAsRollbarData();

        /// <summary>
        /// Gets a value indicating whether to package synchronously (within the logging method call).
        /// 
        /// The logging methods will return very quickly when this flag is off. In the off state, 
        /// the packaging strategy will be invoked during payload transmission on a dedicated worker thread. 
        /// </summary>
        /// <value><c>true</c> if needs to package synchronously; otherwise, <c>false</c>.</value>
        bool MustApplySynchronously { get; }

        /// <summary>
        /// Gets the rollbar data packaged by this strategy (if any).
        /// </summary>
        /// <value>The rollbar data.</value>
        Data? RollbarData { get; }
    }
}
