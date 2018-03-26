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

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    internal class RollbarClient 
    {
        public IRollbarConfig Config { get; }

        public RollbarClient(IRollbarConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            Config = config;
        }

        public RollbarResponse PostAsJson(Payload payload, IEnumerable<string> scrubFields)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            var task = this.PostAsJsonAsync(payload, scrubFields);

            task.Wait();

            return task.Result;
        }

        private readonly IterativeTruncationStrategy _payloadTruncationStrategy = new IterativeTruncationStrategy();

        public async Task<RollbarResponse> PostAsJsonAsync(Payload payload, IEnumerable<string> scrubFields)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            if (this._payloadTruncationStrategy.Truncate(payload) > this._payloadTruncationStrategy.MaxPayloadSizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(payload),
                    message: $"Payload size exceeds {this._payloadTruncationStrategy.MaxPayloadSizeInBytes} bytes limit!"
                    );
            }

            using (var httpClient = this.BuildWebClient())
            {
                var jsonData = JsonConvert.SerializeObject(payload);
                jsonData = ScrubPayload(jsonData, scrubFields);

                httpClient.DefaultRequestHeaders
                    .Add("X-Rollbar-Access-Token", payload.AccessToken);

                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

                var postPayload = 
                    new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header
                var uri = new Uri($"{Config.EndPoint}item/");
                var postResponse = await httpClient.PostAsync(uri, postPayload);

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
        }

        private string ScrubPayload(string payload, IEnumerable<string> scrubFields)
        {
            var jObj = JsonScrubber.CreateJsonObject(payload);
            var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");
            JsonScrubber.ScrubJson(dataProperty, scrubFields);
            var scrubbedPayload = jObj.ToString();
            return scrubbedPayload;
        }

        private HttpClient BuildWebClient()
        {
            HttpClient webClient = null;

            var webProxy = this.BuildWebProxy();

            if (webProxy != null)
            {
                var httpHandler = new HttpClientHandler();
                httpHandler.Proxy = webProxy;
                httpHandler.PreAuthenticate = true;
                httpHandler.UseDefaultCredentials = false;

                webClient = new HttpClient(httpHandler);
            }
            else
            {
                webClient = new HttpClient();
            }

            return webClient;
        }

        private IWebProxy BuildWebProxy()
        {
            if (!string.IsNullOrWhiteSpace(this.Config.ProxyAddress))
            {
                return new WebProxy(this.Config.ProxyAddress);
            }

            return null;
        }



        //for reference: https://blog.jayway.com/2012/03/13/httpclient-makes-get-and-post-very-simple/
        // or https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client

        private const string deployApiPath = @"deploy/";
        public async Task<RollbarResponse> PostAsync(Deployment deployment)
        {
            using (var httpClient = this.BuildWebClient())
            {
                var uri = new Uri(this.Config.EndPoint + RollbarClient.deployApiPath);

                var parameters = new Dictionary<string, string> {
                    { "access_token", (!string.IsNullOrWhiteSpace(deployment.WriteAccessToken)) ? deployment.WriteAccessToken : this.Config.AccessToken },
                    { "environment", (!string.IsNullOrWhiteSpace(deployment.Environment)) ? deployment.Environment : this.Config.Environment  },
                    { "revision", deployment.Revision },
                    { "rollbar_username", deployment.RollbarUsername },
                    { "local_username", deployment.LocalUsername },
                    { "comment", deployment.Comment },
                };
                var httpContent = new FormUrlEncodedContent(parameters);
                var postResponse = await httpClient.PostAsync(uri, httpContent);//.ConfigureAwait(false);

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
        }

        public async Task<DeployResponse> GetDeploymentAsync(string readAccessToken, string deploymentID)
        {
            using (var httpClient = this.BuildWebClient())
            {
                var uri = new Uri(
                    this.Config.EndPoint 
                    + RollbarClient.deployApiPath + deploymentID + @"/" 
                    + $"?access_token={readAccessToken}"
                    );

                var httpResponse = await httpClient.GetAsync(uri);//.ConfigureAwait(false);

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
        public async Task<DeploysPageResponse> GetDeploymentsAsync(string readAccessToken, int pageNumber = 1)
        {
            using (var httpClient = this.BuildWebClient())
            {
                var uri = new Uri(
                    this.Config.EndPoint 
                    + RollbarClient.deploysQueryApiPath
                    + $"?access_token={readAccessToken}&page={pageNumber}"
                    );

                var httpResponse = await httpClient.GetAsync(uri);//.ConfigureAwait(false);

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

    }
}
