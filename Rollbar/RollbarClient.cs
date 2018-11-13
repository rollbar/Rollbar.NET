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
        private readonly RollbarConfig _config;
        private readonly HttpClient _httpClient;
        private readonly Uri _payloadPostUri;
        private readonly IterativeTruncationStrategy _payloadTruncationStrategy;

        public RollbarClient(RollbarConfig config, HttpClient httpClient)
        {
            Assumption.AssertNotNull(config, nameof(config));
            Assumption.AssertNotNull(httpClient, nameof(httpClient));

            this._config = config;

            this._payloadPostUri = new Uri($"{this._config.EndPoint}item/");


            this._httpClient = httpClient;

            var header = new MediaTypeWithQualityHeaderValue("application/json");
            if (!this._httpClient.DefaultRequestHeaders.Accept.Contains(header))
            {
                this._httpClient.DefaultRequestHeaders.Accept.Add(header);
            }

            var sp = ServicePointManager.FindServicePoint(new Uri(this._config.EndPoint));
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

        public RollbarConfig Config { get { return this._config; } }

        public RollbarResponse PostAsJson(Payload payload)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            var task = this.PostAsJsonAsync(payload);

            task.Wait();

            return task.Result;
        }

        public async Task<RollbarResponse> PostAsJsonAsync(Payload payload)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            if (this._payloadTruncationStrategy.Truncate(payload) > this._payloadTruncationStrategy.MaxPayloadSizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(payload),
                    message: $"Payload size exceeds {this._payloadTruncationStrategy.MaxPayloadSizeInBytes} bytes limit!"
                    );
            }

            var jsonData = JsonConvert.SerializeObject(payload);
            jsonData = ScrubPayload(jsonData, this._config.GetSafeScrubFields());

            var postPayload =
                new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header

            Assumption.AssertTrue(string.Equals(payload.AccessToken, this._config.AccessToken), nameof(payload.AccessToken));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._payloadPostUri);
            const string accessTokenHeader = "X-Rollbar-Access-Token";
            request.Headers.Add(accessTokenHeader, this._config.AccessToken);
            request.Content = postPayload;

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

        private string ScrubPayload(string payload, IEnumerable<string> scrubFields)
        {
            var jObj = JsonScrubber.CreateJsonObject(payload);
            var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");
            JsonScrubber.ScrubJson(dataProperty, scrubFields);
            var scrubbedPayload = jObj.ToString();
            return scrubbedPayload;
        }
    }
}
