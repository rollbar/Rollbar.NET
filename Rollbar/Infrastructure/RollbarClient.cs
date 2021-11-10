namespace Rollbar.Infrastructure
{
    using System;
    using System.Threading;
    using System.Net.Http;

    using Rollbar.Diagnostics;

    /// <summary>
    /// Class RollbarClient for accessing the Rollbar API server.
    /// Implements the <see cref="Rollbar.Infrastructure.RollbarClientBase" />
    /// </summary>
    /// <seealso cref="Rollbar.Infrastructure.RollbarClientBase" />
    internal class RollbarClient
        : RollbarClientBase
    {
        #region fields

        /// <summary>
        /// The expected post to API timeout
        /// </summary>
        private readonly TimeSpan _expectedPostToApiTimeout;

        #endregion fields

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClient"/> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        public RollbarClient(IRollbar rollbarLogger)
            : this(rollbarLogger.Config)
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));

            this._rollbarLogger = rollbarLogger;

            this._payloadTruncator = new(rollbarLogger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClient"/> class.
        /// </summary>
        /// <param name="rollbarLoggerConfig">The rollbar logger configuration.</param>
        public RollbarClient(IRollbarLoggerConfig rollbarLoggerConfig)
            : base(rollbarLoggerConfig)
        {
            var httpClient = 
                RollbarQueueController.Instance?.ProvideHttpClient(
                    rollbarLoggerConfig.HttpProxyOptions.ProxyAddress,
                    rollbarLoggerConfig.HttpProxyOptions.ProxyUsername,
                    rollbarLoggerConfig.HttpProxyOptions.ProxyPassword
                );
            this.Set(httpClient);

            this._expectedPostToApiTimeout = 
                RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.PayloadPostTimeout;
        }

        #endregion constructors

        /// <summary>
        /// Posts as json.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>System.Nullable&lt;RollbarResponse&gt;.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">Preliminary ConnectivityMonitor detected offline status!</exception>
        public RollbarResponse? PostAsJson(
            PayloadBundle payloadBundle
            )
        {
            Assumption.AssertNotNull(payloadBundle, nameof(payloadBundle));

            // first, let's run quick Internet availability check
            // to minimize potential timeout of the following JSON POST call: 
            if (RollbarConnectivityMonitor.Instance != null 
                && !RollbarConnectivityMonitor.Instance.IsConnectivityOn
                )
            {
                throw new HttpRequestException("Preliminary ConnectivityMonitor detected offline status!");
            }

            using CancellationTokenSource cancellationTokenSource = new();
            var task = this.PostAsJsonAsync(payloadBundle, cancellationTokenSource.Token);
            try
            {
                if (!task.Wait(this._expectedPostToApiTimeout))
                {
                    cancellationTokenSource.Cancel(true);
                }
                return task.Result;
            }
            catch (System.Exception ex)
            {
                RollbarErrorUtility.Report(
                    null,
                    payloadBundle.AsHttpContentToSend,
                    InternalRollbarError.PayloadPostError,
                    "While PostAsJson(PayloadBundle payloadBundle)...",
                    ex,
                    payloadBundle
                );
                return null;
            }
        }

        /// <summary>
        /// Posts as json.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>System.Nullable&lt;RollbarResponse&gt;.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// Preliminary ConnectivityMonitor detected offline status!
        /// </exception>
        public RollbarResponse? PostAsJson(
            string? accessToken, 
            string? jsonContent
            ) 
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNullOrWhiteSpace(jsonContent, nameof(jsonContent));

            if(string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            // first, let's run quick Internet availability check
            // to minimize potential timeout of the following JSON POST call: 
            if (RollbarConnectivityMonitor.Instance != null 
                && !RollbarConnectivityMonitor.Instance.IsConnectivityOn
                )
            {
                throw new HttpRequestException("Preliminary ConnectivityMonitor detected offline status!");
            }

            using CancellationTokenSource cancellationTokenSource = new();
            var task = this.PostAsJsonAsync(accessToken!, jsonContent!, cancellationTokenSource.Token);
            try
            {
                if (!task.Wait(this._expectedPostToApiTimeout))
                {
                    cancellationTokenSource.Cancel(true);
                }
                return task.Result;
            }
            catch (System.Exception ex)
            {
                RollbarErrorUtility.Report(
                    null,
                    jsonContent,
                    InternalRollbarError.PayloadPostError,
                    "While PostAsJson((string destinationUri, string accessToken, string jsonContent)...",
                    ex,
                    null
                );
                return null;
            }

        }
    }
}
