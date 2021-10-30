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
    internal class RollbarBlazorClient
    {
        #region fields

        /// <summary>
        /// The rollbar logger
        /// </summary>
        private readonly IRollbar _rollbarLogger;

        /// <summary>
        /// The HTTP client
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The payload post URI
        /// </summary>
        private readonly Uri _payloadPostUri;

        /// <summary>
        /// The payload truncation strategy
        /// </summary>
        private readonly IterativeTruncationStrategy _payloadTruncationStrategy;

        /// <summary>
        /// The payload scrubber
        /// </summary>
        private readonly RollbarPayloadScrubber _payloadScrubber;

        #endregion fields

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rollbar.RollbarBlazorClient" /> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        public RollbarBlazorClient(IRollbar logger, HttpClient httpClient)
        {
            Assumption.AssertNotNull(logger, nameof(logger));
            Assumption.AssertNotNull(logger.Config, nameof(logger.Config));

            this._rollbarLogger = logger;

            this._payloadTruncationStrategy = new IterativeTruncationStrategy();
            this._payloadScrubber = new RollbarPayloadScrubber(logger.Config.RollbarDataSecurityOptions.GetFieldsToScrub());

            this._payloadPostUri =
                new Uri($"{logger.Config.RollbarDestinationOptions.EndPoint}item/");

            this._httpClient = httpClient;

            var header = new MediaTypeWithQualityHeaderValue("application/json");
            if(!this._httpClient.DefaultRequestHeaders.Accept.Contains(header))
            {
                this._httpClient.DefaultRequestHeaders.Accept.Add(header);
            }

            var sp = ServicePointManager.FindServicePoint(
                new Uri(logger.Config.RollbarDestinationOptions.EndPoint!)
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
        #endregion constructors

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarLoggerConfig Config { get { return this._rollbarLogger.Config; } }

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

            Payload? payload = payloadBundle.GetPayload();
            Assumption.AssertNotNull(payload, nameof(payload));

            if (!TruncatePayload(payloadBundle))
            {
                return false;
            }

            if (!ScrubHttpMessages(payloadBundle))
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
                jsonData = ScrubPayload(jsonData!);
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
            _ = Assumption.AssertTrue(
                string.Equals(payload!.AccessToken, this._rollbarLogger.Config.RollbarDestinationOptions.AccessToken, StringComparison.Ordinal), 
                nameof(payload.AccessToken)
                );

            return true;
        }

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        public async Task<RollbarResponse?> PostAsJsonAsync(
            PayloadBundle payloadBundle
            )
        {
            Assumption.AssertNotNull(payloadBundle, nameof(payloadBundle));

            // make sure there anything meaningful to send:
            if (string.IsNullOrWhiteSpace(this._rollbarLogger.Config.RollbarDestinationOptions.AccessToken) 
                || !EnsureHttpContentToSend(payloadBundle))
            {
                return null;
            }

            return await PostAsJsonAsync(
                this._rollbarLogger.Config.RollbarDestinationOptions.AccessToken!,
                payloadBundle.AsHttpContentToSend!
                );
        }

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        public async Task<RollbarResponse?> PostAsJsonAsync(
            string accessToken, 
            string jsonContent 
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNullOrWhiteSpace(jsonContent, nameof(jsonContent));

            return await PostAsJsonAsync(
                accessToken, 
                new StringContent(jsonContent)
                );
        }

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="jsonContent">Content of the json.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        private async Task<RollbarResponse?> PostAsJsonAsync(
            string accessToken, 
            StringContent jsonContent 
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNull(jsonContent, nameof(jsonContent));

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
                postResponse = await this._httpClient.SendAsync(request);

                if(postResponse.IsSuccessStatusCode)
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

        #region payload truncation

        /// <summary>
        /// Truncates the payload.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns><c>true</c> if successfully truncated, <c>false</c> otherwise.</returns>
        private bool TruncatePayload(PayloadBundle payloadBundle)
        {
            if (this._payloadTruncationStrategy == null)
            {
                return true; // as far as truncation is concerned - everything is good...
            }

            Payload? payload = payloadBundle.GetPayload();

            if (this._payloadTruncationStrategy.Truncate(payload) > this._payloadTruncationStrategy.MaxPayloadSizeInBytes)
            {
                var exception = new ArgumentOutOfRangeException(
                    paramName: nameof(payloadBundle),
                    message: $"Bundle's payload size exceeds {this._payloadTruncationStrategy.MaxPayloadSizeInBytes} bytes limit!"
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
        /// <returns><c>true</c> if scrubbed successfully, <c>false</c> otherwise.</returns>
        private bool ScrubHttpMessages(PayloadBundle payloadBundle)
        {
            Payload? payload = payloadBundle.GetPayload();
            if(payload == null)
            {
                return true;
            }

            DTOs.Request? request = payload.Data.Request;
            if (request?.PostBody is string requestBody 
                && request.Headers != null
                && request.Headers.TryGetValue("Content-Type", out string? requestContentTypeHeader)
                )
            {
                request.PostBody =
                    this.ScrubHttpMessageBodyContentString(
                        requestBody,
                        requestContentTypeHeader,
                        this._payloadScrubber.ScrubMask,
                        this._payloadScrubber.PayloadFieldNames,
                        this._payloadScrubber.HttpRequestBodyPaths);
            }

            DTOs.Response? response = payload.Data.Response;
            if (response?.Body is string responseBody 
                && response.Headers != null
                && response.Headers.TryGetValue("Content-Type", out string? responseContentTypeHeader)
                )
            {
                response.Body =
                    this.ScrubHttpMessageBodyContentString(
                        responseBody,
                        responseContentTypeHeader,
                        this._payloadScrubber.ScrubMask,
                        this._payloadScrubber.PayloadFieldNames,
                        this._payloadScrubber.HttpResponseBodyPaths);
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
        private string? ScrubHttpMessageBodyContentString(
            string body, 
            string contentTypeHeaderValue, 
            string scrubMask, 
            string[] scrubFields, 
            string[] scrubPaths
            )
        {
            string contentType = contentTypeHeaderValue.ToLower();
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
        }

        #endregion payload scrubbing
    }
}
