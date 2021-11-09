namespace Rollbar.Infrastructure
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Newtonsoft.Json;

    using Rollbar.DTOs;
    using Rollbar.Diagnostics;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    internal class RollbarBlazorClient
        : RollbarClientBase
    {
        #region fields

        #endregion fields

        #region constructors

        public RollbarBlazorClient(IRollbar rollbarLogger, HttpClient httpClient)
            : base(rollbarLogger)
        {
            Assumption.AssertNotNull(httpClient, nameof(httpClient));

            this.Set(httpClient);
        }

        #endregion constructors

    }
}
