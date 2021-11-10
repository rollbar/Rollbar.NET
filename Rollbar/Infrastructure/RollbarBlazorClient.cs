namespace Rollbar.Infrastructure
{
    using System.Net.Http;

    using Rollbar.Diagnostics;

    /// <summary>
    /// Class RollbarBlazorClient for accessing the Rollbar API server from Blazor-client/WASM specifically.
    /// Implements the <see cref="Rollbar.Infrastructure.RollbarClientBase" />
    /// </summary>
    /// <seealso cref="Rollbar.Infrastructure.RollbarClientBase" />
    internal class RollbarBlazorClient
        : RollbarClientBase
    {
        #region fields

        #endregion fields

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarBlazorClient"/> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public RollbarBlazorClient(IRollbar rollbarLogger, HttpClient httpClient)
            : base(rollbarLogger)
        {
            Assumption.AssertNotNull(httpClient, nameof(httpClient));

            this.Set(httpClient);
        }

        #endregion constructors
    }
}
