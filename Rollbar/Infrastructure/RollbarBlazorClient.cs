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
    {
        #region fields

        /// <summary>
        /// The HTTP client
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The rollbar logger
        /// </summary>
        private readonly IRollbar _rollbarLogger;

        /// <summary>
        /// The payload post URI
        /// </summary>
        private readonly Uri _payloadPostUri;

        private readonly RollbarPayloadTruncator _payloadTruncator;

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

            this._payloadTruncator = new(logger);
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
            catch (NotImplementedException)
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

            if (!this._payloadTruncator.TruncatePayload(payloadBundle))
            {
                return false;
            }

            if (!this._payloadScrubber.ScrubHttpMessages(payloadBundle))
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

    }
}
