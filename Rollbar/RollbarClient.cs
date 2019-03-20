[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Newtonsoft.Json;

    using Rollbar.DTOs;
    using Rollbar.Diagnostics;
    using Rollbar.Serialization.Json;
    using Rollbar.PayloadTruncation;

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    internal class RollbarClient 
    {
        /// <summary>
        /// The rollbar logger
        /// </summary>
        private readonly RollbarLogger _rollbarLogger;
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
        /// Initializes a new instance of the <see cref="RollbarClient"/> class.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        public RollbarClient(RollbarLogger rollbarLogger)
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));
            Assumption.AssertNotNull(rollbarLogger.Config, nameof(rollbarLogger.Config));

            this._rollbarLogger = rollbarLogger;

            this._payloadPostUri = 
                new Uri($"{this._rollbarLogger.Config.EndPoint}item/");
            this._httpClient = 
                RollbarQueueController.Instance.ProvideHttpClient(
                    this._rollbarLogger.Config.ProxyAddress,
                    this._rollbarLogger.Config.ProxyUsername,
                    this._rollbarLogger.Config.ProxyPassword
                    );

            var header = new MediaTypeWithQualityHeaderValue("application/json");
            if (!this._httpClient.DefaultRequestHeaders.Accept.Contains(header))
            {
                this._httpClient.DefaultRequestHeaders.Accept.Add(header);
            }

            var sp = ServicePointManager.FindServicePoint(new Uri(this._rollbarLogger.Config.EndPoint));
            try
            {
                sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (NotImplementedException ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // just a crash prevention.
                // this is a work around the unimplemented property within Mono runtime...
            }

            this._payloadTruncationStrategy = new IterativeTruncationStrategy();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IRollbarConfig Config { get { return this._rollbarLogger.Config; } }

        /// <summary>
        /// Posts as json.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>RollbarResponse.</returns>
        public RollbarResponse PostAsJson(PayloadBundle payloadBundle)
        {
            Assumption.AssertNotNull(payloadBundle, nameof(payloadBundle));

            var task = this.PostAsJsonAsync(payloadBundle);

            task.Wait();

            return task.Result;
        }

        /// <summary>
        /// post as json as an asynchronous operation.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <returns>Task&lt;RollbarResponse&gt;.</returns>
        public async Task<RollbarResponse> PostAsJsonAsync(PayloadBundle payloadBundle)
        {
            Assumption.AssertNotNull(payloadBundle, nameof(payloadBundle));

            Payload payload = payloadBundle.GetPayload();
            Assumption.AssertNotNull(payload, nameof(payload));

            if (payloadBundle.AsHttpContentToSend == null)
            {
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
                        exception
                        );
                }

                string jsonData = null;
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
                        exception
                        );

                    return null;
                }

                try
                {
                    jsonData = ScrubPayload(jsonData, this._rollbarLogger.Config.GetFieldsToScrub());
                }
                catch (System.Exception exception)
                {
                    RollbarErrorUtility.Report(
                        this._rollbarLogger,
                        payload,
                        InternalRollbarError.PayloadScrubbingError,
                        "While scrubbing a payload...",
                        exception
                        );

                    return null;
                }

                payloadBundle.AsHttpContentToSend =
                    new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header
            }

            Assumption.AssertNotNull(payloadBundle.AsHttpContentToSend, nameof(payloadBundle.AsHttpContentToSend));
            Assumption.AssertTrue(string.Equals(payload.AccessToken, this._rollbarLogger.Config.AccessToken), nameof(payload.AccessToken));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._payloadPostUri);
            const string accessTokenHeader = "X-Rollbar-Access-Token";
            request.Headers.Add(accessTokenHeader, this._rollbarLogger.Config.AccessToken);
            request.Content = payloadBundle.AsHttpContentToSend;

            var postResponse = await this._httpClient.SendAsync(request);

            RollbarResponse response = null;
            if (postResponse.IsSuccessStatusCode)
            {
                string reply = await postResponse.Content.ReadAsStringAsync();
                response = JsonConvert.DeserializeObject<RollbarResponse>(reply);
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

            return response;
        }

        /// <summary>
        /// Scrubs the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="scrubFields">The scrub fields.</param>
        /// <returns>System.String.</returns>
        private static string ScrubPayload(string payload, IEnumerable<string> scrubFields)
        {
            var jObj = JsonScrubber.CreateJsonObject(payload);
            var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");
            JsonScrubber.ScrubJson(dataProperty, scrubFields);
            var scrubbedPayload = jObj.ToString();
            return scrubbedPayload;
        }
    }
}
