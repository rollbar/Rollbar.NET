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
    using Rollbar.Deploys;
    using Rollbar.Common;
    using System.Linq;

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
            sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute


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

            const string accessTokenHeader = "X-Rollbar-Access-Token";
            //if (!this._httpClient.DefaultRequestHeaders.Contains(accessTokenHeader))
            //{
            //    this._httpClient.DefaultRequestHeaders
            //        .Add(accessTokenHeader, this._config.AccessToken);
            //}
            //else
            //{
            //    var accessTokenHeaderValues = this._httpClient.DefaultRequestHeaders.GetValues(accessTokenHeader).ToArray();
            //    Assumption.AssertEqual(accessTokenHeaderValues.Length, 1, nameof(accessTokenHeaderValues.Length));
            //    if (!string.Equals(accessTokenHeaderValues.First(), this._config.AccessToken, StringComparison.OrdinalIgnoreCase))
            //    {
            //        this._httpClient.DefaultRequestHeaders.Remove(accessTokenHeader);
            //        this._httpClient.DefaultRequestHeaders
            //            .Add(accessTokenHeader, this._config.AccessToken);
            //    }
            //}

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this._payloadPostUri);
            request.Headers.Add(accessTokenHeader, this._config.AccessToken);
            request.Content = postPayload;

            //var postResponse = 
            //    await this._httpClient.PostAsync(this._payloadPostUri, postPayload);
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

        #region deployment

        private const string deployApiPath = @"deploy/";

        /// <summary>
        /// Posts the specified deployment asynchronously.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns></returns>
        public async Task PostAsync(IDeployment deployment)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));
            Assumption.AssertNotNullOrWhiteSpace(this._config.AccessToken, nameof(this._config.AccessToken));
            Assumption.AssertFalse(string.IsNullOrWhiteSpace(deployment.Environment) && string.IsNullOrWhiteSpace(this._config.Environment), nameof(deployment.Environment));
            Assumption.AssertNotNullOrWhiteSpace(deployment.Revision, nameof(deployment.Revision));

            Assumption.AssertLessThan(
                deployment.Environment.Length, 256,
                nameof(deployment.Environment.Length)
                );
            Assumption.AssertTrue(
                deployment.LocalUsername == null || deployment.LocalUsername.Length < 256,
                nameof(deployment.LocalUsername)
                );
            Assumption.AssertTrue(
                deployment.Comment == null || StringUtility.CalculateExactEncodingBytes(deployment.Comment, Encoding.UTF8) <= (62 * 1024),
                nameof(deployment.Comment)
                );

            using (var httpClient = HttpClientUtil.CreateHttpClient(this._config.ProxyAddress))
            {
                var uri = new Uri(this._config.EndPoint + RollbarClient.deployApiPath);

                var parameters = new Dictionary<string, string> {
                    { "access_token", this._config.AccessToken },
                    { "environment", (!string.IsNullOrWhiteSpace(deployment.Environment)) ? deployment.Environment : this._config.Environment  },
                    { "revision", deployment.Revision },
                    { "rollbar_username", deployment.RollbarUsername },
                    { "local_username", deployment.LocalUsername },
                    { "comment", deployment.Comment },
                };
                var httpContent = new FormUrlEncodedContent(parameters);
                var postResponse = await httpClient.PostAsync(uri, httpContent);

                if (postResponse.IsSuccessStatusCode)
                {
                    string reply = await postResponse.Content.ReadAsStringAsync();
                }
                else
                {
                    postResponse.EnsureSuccessStatusCode();
                }

                return;
            }
        }

        /// <summary>
        /// Gets the deployment asynchronous.
        /// </summary>
        /// <param name="readAccessToken">The read access token.</param>
        /// <param name="deploymentID">The deployment identifier.</param>
        /// <returns></returns>
        public async Task<DeployResponse> GetDeploymentAsync(string readAccessToken, string deploymentID)
        {
            Assumption.AssertNotNullOrWhiteSpace(readAccessToken, nameof(readAccessToken));
            Assumption.AssertNotNullOrWhiteSpace(deploymentID, nameof(deploymentID));

            using (var httpClient = HttpClientUtil.CreateHttpClient(this._config.ProxyAddress))
            {
                var uri = new Uri(
                    this._config.EndPoint 
                    + RollbarClient.deployApiPath + deploymentID + @"/" 
                    + $"?access_token={readAccessToken}"
                    );

                var httpResponse = await httpClient.GetAsync(uri);

                DeployResponse response = null;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string reply = await httpResponse.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<DeployResponse>(reply);
                }
                else
                {
                    httpResponse.EnsureSuccessStatusCode();
                }

                return response;
            }
        }

        private const string deploysQueryApiPath = @"deploys/";

        /// <summary>
        /// Gets the deployments asynchronous.
        /// </summary>
        /// <param name="readAccessToken">The read access token.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns></returns>
        public async Task<DeploysPageResponse> GetDeploymentsAsync(string readAccessToken, int pageNumber = 1)
        {
            Assumption.AssertNotNullOrWhiteSpace(readAccessToken, nameof(readAccessToken));

            using (var httpClient = HttpClientUtil.CreateHttpClient(this._config.ProxyAddress))
            {
                var uri = new Uri(
                    this._config.EndPoint 
                    + RollbarClient.deploysQueryApiPath
                    + $"?access_token={readAccessToken}&page={pageNumber}"
                    );

                var httpResponse = await httpClient.GetAsync(uri);

                DeploysPageResponse response = null;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string reply = await httpResponse.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<DeploysPageResponse>(reply);
                }
                else
                {
                    httpResponse.EnsureSuccessStatusCode();
                }

                return response;
            }
        }

        #endregion deployment
    }
}
