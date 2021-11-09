namespace Rollbar.Infrastructure
{
    using System;
    using System.Text;
    using System.Threading;
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
    internal class RollbarClient
        : RollbarClientBase
    {
        #region fields

        private readonly TimeSpan _expectedPostToApiTimeout;

        #endregion fields

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClient" /> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        public RollbarClient(IRollbar rollbarLogger)
            : this(rollbarLogger.Config)
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));

            this._rollbarLogger = rollbarLogger;

            this._payloadTruncator = new(rollbarLogger);
        }

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

            using(CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var task = this.PostAsJsonAsync(payloadBundle, cancellationTokenSource.Token);

                try
                {
                    if(!task.Wait(this._expectedPostToApiTimeout))
                    {
                        cancellationTokenSource.Cancel(true);
                    }
                    return task.Result;
                }
                catch(System.Exception ex)
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
        }

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

            using(CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var task = this.PostAsJsonAsync(accessToken!, jsonContent!, cancellationTokenSource.Token);

                try
                {
                    if (!task.Wait(this._expectedPostToApiTimeout))
                    {
                        cancellationTokenSource.Cancel(true);
                    }
                    return task.Result;
                }
                catch(System.Exception ex)
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
}
