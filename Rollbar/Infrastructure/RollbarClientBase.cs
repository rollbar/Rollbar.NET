namespace Rollbar.Infrastructure
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Runtime.ExceptionServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarClientBase for accessing the Rollbar API server.
    /// 
    /// The base for building differnt flavors of Rollbar clients.
    /// </summary>
    internal abstract class RollbarClientBase
    {
        #region fields

        /// <summary>
        /// The payload truncator
        /// </summary>
        protected RollbarPayloadTruncator? _payloadTruncator;

        /// <summary>
        /// The payload scrubber
        /// </summary>
        protected readonly RollbarPayloadScrubber? _payloadScrubber;


        /// <summary>
        /// The rollbar logger
        /// </summary>
        protected IRollbar? _rollbarLogger;

        /// <summary>
        /// The rollbar logger configuration
        /// </summary>
        protected readonly IRollbarLoggerConfig _rollbarLoggerConfig;

        /// <summary>
        /// The payload post URI
        /// </summary>
        protected readonly Uri _payloadPostUri;

        /// <summary>
        /// The HTTP client
        /// </summary>
        private HttpClient? _httpClient;

        #endregion fields

        #region constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarClientBase"/> class from being created.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private RollbarClientBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClientBase"/> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        protected RollbarClientBase(IRollbar rollbarLogger)
            : this(rollbarLogger.Config)
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));

            this._rollbarLogger = rollbarLogger;

            this._payloadTruncator = new(rollbarLogger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClientBase"/> class.
        /// </summary>
        /// <param name="rollbarLoggerConfig">The rollbar logger configuration.</param>
        /// <exception cref="Rollbar.RollbarException">
        /// Undefined destination end-point!
        /// </exception>
        protected RollbarClientBase(IRollbarLoggerConfig rollbarLoggerConfig)
        {
            Assumption.AssertNotNull(
                rollbarLoggerConfig, 
                nameof(rollbarLoggerConfig)
                );
            Assumption.AssertNotNullOrEmpty(
                rollbarLoggerConfig.RollbarDestinationOptions.EndPoint, 
                nameof(rollbarLoggerConfig.RollbarDestinationOptions.EndPoint)
                );

            if (string.IsNullOrWhiteSpace(rollbarLoggerConfig.RollbarDestinationOptions.EndPoint))
            {
                throw new RollbarException(InternalRollbarError.InfrastructureError, "Undefined destination end-point!");
            }

            this._rollbarLoggerConfig = rollbarLoggerConfig;

            this._payloadPostUri =
                new Uri($"{rollbarLoggerConfig.RollbarDestinationOptions.EndPoint}item/");

            var sp =
                ServicePointManager.FindServicePoint(
                    new Uri(rollbarLoggerConfig.RollbarDestinationOptions.EndPoint)
                    );
            try
            {
                sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
            }
            catch (NotImplementedException)
            {
                // just a crash prevention.
                // this is a work around the unimplemented property within Mono runtime...
            }

            this._payloadScrubber = new(rollbarLoggerConfig.RollbarDataSecurityOptions.GetFieldsToScrub());

            this._payloadTruncator = new(null);
        }

        #endregion constructors

        #region properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarLoggerConfig Config
        {
            get
            {
                return this._rollbarLoggerConfig;
            }
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <value>The HTTP client.</value>
        protected HttpClient? HttpClient
        {
            get
            {
                return this._httpClient;
            }
        }

        #endregion properties

        #region static methods

        /// <summary>
        /// Customizes the specified HTTP client.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        protected static void Customize(HttpClient? httpClient)
        {
            if (httpClient == null)
                return;

            var header = new MediaTypeWithQualityHeaderValue("application/json");
            if (!httpClient.DefaultRequestHeaders.Accept.Contains(header))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(header);
            }
        }

        #endregion static methods

        #region methods

        /// <summary>
        /// Ensures the HTTP content to send.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool EnsureHttpContentToSend(PayloadBundle payloadBundle)
        {
            if (payloadBundle.AsHttpContentToSend != null)
            {
                return true;
            }

            Payload? payload = payloadBundle.GetPayload();
            Assumption.AssertNotNull(payload, nameof(payload));

            if (this._payloadTruncator == null 
                || !this._payloadTruncator.TruncatePayload(payloadBundle)
                )
            {
                return false;
            }

            if (this._payloadScrubber == null 
                || !this._payloadScrubber.ScrubHttpMessages(payloadBundle)
                )
            {
                return false;
            }

            string? jsonData = SerializePayloadAsJsonString(payloadBundle);
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return false;
            }

            try
            {
                jsonData = this._payloadScrubber.ScrubPayload(jsonData!);
            }
            catch (System.Exception exception)
            {
                RollbarErrorUtility.Report(
                    this._rollbarLogger,
                    payload,
                    InternalRollbarError.PayloadScrubbingError,
                    "While scrubbing a payload...",
                    exception,
                    payloadBundle
                    );

                return false;
            }

            payloadBundle.AsHttpContentToSend =
                new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header

            Assumption.AssertNotNull(
                payloadBundle.AsHttpContentToSend,
                nameof(payloadBundle.AsHttpContentToSend)
                );
#pragma warning disable CA1307 // Specify StringComparison for clarity
            _ = Assumption.AssertTrue(
                string.Equals(payload!.AccessToken, this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken),
                nameof(payload.AccessToken)
                );
#pragma warning restore CA1307 // Specify StringComparison for clarity

            return true;
        }

        /// <summary>
        /// Post as json as an asynchronous operation.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <param name="cancellationToken">
        /// The cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// </param>
        /// <returns>A Task&lt;RollbarResponse&gt; representing the asynchronous operation.</returns>
        public async Task<RollbarResponse?> PostAsJsonAsync(
            PayloadBundle payloadBundle,
            CancellationToken? cancellationToken = null
            )
        {
            Assumption.AssertNotNull(payloadBundle, nameof(payloadBundle));

            // make sure there anything meaningful to send:
            if (string.IsNullOrWhiteSpace(this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken)
                || !EnsureHttpContentToSend(payloadBundle)
                )
            {
                return null;
            }

            return await PostAsJsonAsync(
                this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken!,
                payloadBundle.AsHttpContentToSend!,
                cancellationToken
                );
        }

        /// <summary>
        /// Post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <param name="cancellationToken">
        /// The cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// </param>
        /// <returns>A Task&lt;RollbarResponse&gt; representing the asynchronous operation.</returns>
        public async Task<RollbarResponse?> PostAsJsonAsync(
            string accessToken,
            string jsonContent,
            CancellationToken? cancellationToken = null
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNullOrWhiteSpace(jsonContent, nameof(jsonContent));

            return await PostAsJsonAsync(
                accessToken,
                new StringContent(jsonContent),
                cancellationToken
                );
        }

        /// <summary>
        /// Sets the specified HTTP client.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        protected void Set(HttpClient? httpClient)
        {
            Customize(httpClient);
            this._httpClient = httpClient;
        }

        /// <summary>
        /// Post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <param name="cancellationToken">
        /// The cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// </param>
        /// <returns>A Task&lt;RollbarResponse&gt; representing the asynchronous operation.</returns>
        private async Task<RollbarResponse?> PostAsJsonAsync(
            string accessToken,
            StringContent jsonContent,
            CancellationToken? cancellationToken = null
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNull(jsonContent, nameof(jsonContent));

            if (this._httpClient == null)
            {
                return null;
            }

            // build an HTTP request:
            HttpRequestMessage request = new(HttpMethod.Post, this._payloadPostUri);
            const string accessTokenHeader = "X-Rollbar-Access-Token";
            request.Headers.Add(accessTokenHeader, accessToken);
            request.Content = jsonContent;

            // send the request:
            HttpResponseMessage? postResponse = null;
            RollbarResponse? response = null;
            try
            {
                if (cancellationToken != null)
                {
                    postResponse = await this._httpClient.SendAsync(request, (CancellationToken) cancellationToken);
                }
                else
                {
                    postResponse = await this._httpClient.SendAsync(request);
                }

                if (postResponse.IsSuccessStatusCode)
                {
                    string reply =
                        await postResponse.Content.ReadAsStringAsync();
                    response =
                        JsonConvert.DeserializeObject<RollbarResponse>(reply);
                    if (response != null)
                    {
                        response.RollbarRateLimit =
                            new RollbarRateLimit(postResponse.Headers);
                        response.HttpDetails =
                            $"Response: {postResponse}"
                            + Environment.NewLine
                            + $"Request: {postResponse.RequestMessage}"
                            + Environment.NewLine
                            ;
                    }
                }
                else
                {
                    postResponse.EnsureSuccessStatusCode();
                }
            }
            catch (System.Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();  // we are waiting outside of this method...
            }
            finally
            {
                postResponse?.Dispose();
            }

            return response;
        }

        /// <summary>
        /// Serializes the payload as json string.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        private string? SerializePayloadAsJsonString(PayloadBundle payloadBundle)
        {
            Payload? payload = payloadBundle.GetPayload();

            string jsonData;
            try
            {
                jsonData = JsonConvert.SerializeObject(payload);
            }
            catch (System.Exception exception)
            {
                RollbarErrorUtility.Report(
                    this._rollbarLogger,
                    payload,
                    InternalRollbarError.PayloadSerializationError,
                    "While serializing a payload...",
                    exception,
                    payloadBundle
                );

                return null;
            }

            return jsonData;
        }

        #endregion methods
    }
}
