namespace Rollbar
{
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    /// <summary>
    /// Interface IRollbarPackagingStrategy
    /// </summary>
    public interface IRollbarPackagingStrategy
    {
        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null.</returns>
        Data PackageAsRollbarData();

        /// <summary>
        /// Packages as rollbar data asynchronously.
        /// </summary>
        /// <returns>Task&lt;Data&gt; producing Rollbar Data DTO or null.</returns>
        Task<Data> PackageAsRollbarDataAsync();
    }
}
