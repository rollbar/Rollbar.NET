namespace Rollbar
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
    using Rollbar.PayloadTruncation;
    using Rollbar.PayloadScrubbing;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    internal class RollbarClient 
    {
        #region fields

        /// <summary>
        /// The rollbar logger
        /// </summary>
        private readonly IRollbar? _rollbarLogger;

        /// <summary>
        /// The rollbar logger configuration
        /// </summary>
        private readonly IRollbarLoggerConfig _rollbarLoggerConfig;

        /// <summary>
        /// The HTTP client
        /// </summary>
        private readonly HttpClient? _httpClient;

        /// <summary>
        /// The expected post to API timeout
        /// </summary>
        private readonly TimeSpan _expectedPostToApiTimeout;

        /// <summary>
        /// The payload post URI
        /// </summary>
        private readonly Uri _payloadPostUri;

        /// <summary>
        /// The payload truncation strategy
        /// </summary>
        private readonly IterativeTruncationStrategy? _payloadTruncationStrategy;

        /// <summary>
        /// The payload scrubber
        /// </summary>
        private readonly RollbarPayloadScrubber? _payloadScrubber;

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

            this._payloadTruncationStrategy = new IterativeTruncationStrategy();
            this._payloadScrubber = new RollbarPayloadScrubber(this._rollbarLogger.Config.RollbarDataSecurityOptions.GetFieldsToScrub());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarClient"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        public RollbarClient(IRollbarLoggerConfig rollbarConfig)
        {
            Assumption.AssertNotNull(rollbarConfig, nameof(rollbarConfig));

            this._rollbarLoggerConfig = rollbarConfig;

            this._payloadPostUri = 
                new Uri($"{rollbarConfig.RollbarDestinationOptions.EndPoint}item/");
            this._httpClient = 
                RollbarQueueController.Instance?.ProvideHttpClient(
                    rollbarConfig.HttpProxyOptions.ProxyAddress,
                    rollbarConfig.HttpProxyOptions.ProxyUsername,
                    rollbarConfig.HttpProxyOptions.ProxyPassword
                );

            this._expectedPostToApiTimeout = 
                RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.PayloadPostTimeout;

            var header = new MediaTypeWithQualityHeaderValue("application/json");
            if (this._httpClient != null 
                && !this._httpClient.DefaultRequestHeaders.Accept.Contains(header)
                )
            {
                this._httpClient.DefaultRequestHeaders.Accept.Add(header);
            }

            if(!string.IsNullOrWhiteSpace(rollbarConfig.RollbarDestinationOptions.EndPoint))
            {
                var sp = ServicePointManager.FindServicePoint(
                    new Uri(rollbarConfig.RollbarDestinationOptions.EndPoint)
                    );
                try
                {
                    sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
                }
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable IDE0059 // Variable is declared but never used
                catch(NotImplementedException ex)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Variable is declared but never used
                {
                    // just a crash prevention.
                    // this is a work around the unimplemented property within Mono runtime...
                }
            }
        }

        #endregion constructors

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarLoggerConfig Config { get { return this._rollbarLoggerConfig; } }

        /// <summary>
        /// Ensures the HTTP content to send.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if succeeds, <c>false</c> otherwise.</returns>
        public bool EnsureHttpContentToSend(PayloadBundle payloadBundle)
        {
            if (payloadBundle.AsHttpContentToSend != null)
            {
                return true;
            }

            Payload payload = payloadBundle.GetPayload();
            Assumption.AssertNotNull(payload, nameof(payload));

            if (!TruncatePayload(payloadBundle))
            {
                return false;
            }

            if (!ScrubHttpMessages(payloadBundle))
            {
                return false;
            }

            string jsonData = SerializePayloadAsJsonString(payloadBundle);
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return false;
            }

            try
            {
                jsonData = ScrubPayload(jsonData);
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

            Assumption.AssertNotNull(payloadBundle.AsHttpContentToSend, nameof(payloadBundle.AsHttpContentToSend));
#pragma warning disable CA1307 // Specify StringComparison for clarity
            _ = Assumption.AssertTrue(string.Equals(payload.AccessToken, this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken), nameof(payload.AccessToken));
#pragma warning restore CA1307 // Specify StringComparison for clarity

            return true;
        }

        /// <summary>
        /// Posts as json.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>RollbarResponse.</returns>
        public RollbarResponse? PostAsJson(PayloadBundle payloadBundle)
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

        /// <summary>
        /// Posts as json.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>RollbarResponse.</returns>
        public RollbarResponse? PostAsJson(string? accessToken, string jsonContent) 
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
                var task = this.PostAsJsonAsync(accessToken, jsonContent, cancellationTokenSource.Token);

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

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        public async Task<RollbarResponse?> PostAsJsonAsync(
            PayloadBundle payloadBundle, 
            CancellationToken cancellationToken
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
                this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken,
                payloadBundle.AsHttpContentToSend,
                cancellationToken
                );
        }

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        public async Task<RollbarResponse> PostAsJsonAsync(
            string accessToken, 
            string jsonContent, 
            CancellationToken cancellationToken
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
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        private async Task<RollbarResponse?> PostAsJsonAsync(
            string accessToken, 
            StringContent jsonContent, 
            CancellationToken cancellationToken
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNull(jsonContent, nameof(jsonContent));

            if(this._httpClient == null)
            {
                return null;
            }

            // build an HTTP request:
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._payloadPostUri);
            const string accessTokenHeader = "X-Rollbar-Access-Token";
            request.Headers.Add(accessTokenHeader, accessToken);
            request.Content = jsonContent;
            
            // send the request:
            HttpResponseMessage? postResponse = null;
            RollbarResponse? response = null;
            try 
            {
                postResponse = await this._httpClient.SendAsync(request, cancellationToken);

                if(postResponse.IsSuccessStatusCode)
                {
                    string reply = 
                        await postResponse.Content.ReadAsStringAsync();
                    response = 
                        JsonConvert.DeserializeObject<RollbarResponse>(reply);
                    response.RollbarRateLimit = 
                        new RollbarRateLimit(postResponse.Headers);
                    response.HttpDetails =
                        $"Response: {postResponse}"
                        + Environment.NewLine
                        + $"Request: {postResponse.RequestMessage}"
                        + Environment.NewLine
                        ;
                }
                else
                {
                    postResponse.EnsureSuccessStatusCode();
                }

            }
            catch(System.Exception ex)
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
        /// <returns>System.String.</returns>
        private string SerializePayloadAsJsonString(PayloadBundle payloadBundle)
        {
            Payload payload = payloadBundle.GetPayload();

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

        #region payload truncation

        /// <summary>
        /// Truncates the payload.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool TruncatePayload(PayloadBundle payloadBundle)
        {
            if (this._payloadTruncationStrategy == null)
            {
                return true; // as far as truncation is concerned - everything is good...
            }

            Payload payload = payloadBundle.GetPayload();

            if (this._payloadTruncationStrategy.Truncate(payload) > this._payloadTruncationStrategy.MaxPayloadSizeInBytes)
            {
                var exception = new ArgumentOutOfRangeException(
                    paramName: nameof(payload),
                    message: $"Payload size exceeds {this._payloadTruncationStrategy.MaxPayloadSizeInBytes} bytes limit!"
                );

                RollbarErrorUtility.Report(
                    this._rollbarLogger,
                    payload,
                    InternalRollbarError.PayloadTruncationError,
                    "While truncating a payload...",
                    exception,
                    payloadBundle
                );

                return false;
            }

            return true;
        }

        #endregion payload truncation

        #region payload scrubbing

        /// <summary>
        /// Scrubs the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>System.String.</returns>
        internal string ScrubPayload(string payload)
        {
            if (this._payloadScrubber == null)
            {
                return payload; // as far as scrubbing is concerned - everything is good...
            }

            var scrubbedPayload = this._payloadScrubber.ScrubPayload(payload);
            return scrubbedPayload;
        }

        /// <summary>
        /// Scrubs the HTTP messages.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ScrubHttpMessages(PayloadBundle payloadBundle)
        {
            if(this._payloadScrubber == null)
            {
                return true;
            }

            Payload payload = payloadBundle.GetPayload();

            DTOs.Request request = payload.Data.Request;
            if (request?.PostBody is string requestBody)
            {
                if (request.Headers.TryGetValue("Content-Type", out string? contentTypeHeader))
                {
                    request.PostBody = 
                        this.ScrubHttpMessageBodyContentString(
                            requestBody, 
                            contentTypeHeader,
                            this._payloadScrubber.ScrubMask, 
                            this._payloadScrubber.PayloadFieldNames,
                            this._payloadScrubber.HttpRequestBodyPaths);
                }
            }

            DTOs.Response response = payload.Data.Response;
            if (response?.Body is string responseBody)
            {
                if (response.Headers.TryGetValue("Content-Type", out string? contentTypeHeader))
                {
                    response.Body =
                        this.ScrubHttpMessageBodyContentString(
                            responseBody,
                            contentTypeHeader,
                            this._payloadScrubber.ScrubMask,
                            this._payloadScrubber.PayloadFieldNames,
                            this._payloadScrubber.HttpResponseBodyPaths);
                }
            }

            return true;
        }

        /// <summary>
        /// Scrubs the HTTP message body content string.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="contentTypeHeaderValue">The content type header value.</param>
        /// <param name="scrubMask">The scrub mask.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <param name="scrubPaths">The scrub paths.</param>
        /// <returns>System.String.</returns>
        private string ScrubHttpMessageBodyContentString(
            string body, 
            string contentTypeHeaderValue, 
            string scrubMask, 
            string[] scrubFields, 
            string[] scrubPaths
            )
        {
            string contentType = contentTypeHeaderValue.ToLower();
#pragma warning disable CA1307 // Specify StringComparison for clarity
            if (contentType.Contains("json"))
            {
                return new JsonStringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else if (contentType.Contains("xml"))
            {
                return new XmlStringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else if (contentType.Contains("form-data"))
            {
                return new FormDataStringScrubber(contentTypeHeaderValue, scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
            else
            {
                return new StringScrubber(scrubMask, scrubFields, scrubPaths).Scrub(body);
            }
#pragma warning restore CA1307 // Specify StringComparison for clarity
        }

        #endregion payload scrubbing
    }
}
