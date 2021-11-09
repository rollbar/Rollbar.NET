namespace Rollbar.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    internal abstract class RollbarClientBase
    {
        #region fields

        protected RollbarPayloadTruncator? _payloadTruncator;

        protected readonly RollbarPayloadScrubber? _payloadScrubber;


        protected IRollbar? _rollbarLogger;

        protected readonly IRollbarLoggerConfig _rollbarLoggerConfig;

        protected readonly Uri _payloadPostUri;

        private HttpClient? _httpClient;

        #endregion fields

        #region constructors

        private RollbarClientBase()
        {
        }

        protected RollbarClientBase(IRollbar rollbarLogger)
            : this(rollbarLogger.Config)
        {
            Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));

            this._rollbarLogger = rollbarLogger;

            this._payloadTruncator = new(rollbarLogger);
        }

        protected RollbarClientBase(IRollbarLoggerConfig rollbarLoggerConfig)
        {
            Assumption.AssertNotNull(rollbarLoggerConfig, nameof(rollbarLoggerConfig));

            this._rollbarLoggerConfig = rollbarLoggerConfig;

            if (!string.IsNullOrWhiteSpace(rollbarLoggerConfig.RollbarDestinationOptions.EndPoint))
            {
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
            }

            this._payloadScrubber = new(rollbarLoggerConfig.RollbarDataSecurityOptions.GetFieldsToScrub());

            this._payloadTruncator = new(null);
        }

        #endregion constructors

        #region properties

        public IRollbarLoggerConfig Config
        {
            get
            {
                return this._rollbarLoggerConfig;
            }
        }
        
        protected HttpClient? HttpClient
        {
            get
            {
                return this._httpClient;
            }
        }

        //protected RollbarPayloadTruncator? Truncator
        //{
        //    get
        //    {
        //        return this._payloadTruncator;
        //    }
        //}

        //protected RollbarPayloadScrubber? Scrubber 
        //{
        //    get 
        //    {
        //        return this._payloadScrubber;} 
        //}

        #endregion properties

        #region static methods

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
#pragma warning disable CA1307 // Specify StringComparison for clarity
            _ = Assumption.AssertTrue(
                string.Equals(payload!.AccessToken, this._rollbarLoggerConfig.RollbarDestinationOptions.AccessToken),
                nameof(payload.AccessToken)
                );
#pragma warning restore CA1307 // Specify StringComparison for clarity

            return true;
        }

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

        protected void Set(HttpClient? httpClient)
        {
            Customize(httpClient);
            this._httpClient = httpClient;
        }

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._payloadPostUri);
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
