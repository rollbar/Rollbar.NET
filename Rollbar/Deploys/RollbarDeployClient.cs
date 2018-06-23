namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    internal class RollbarDeployClient
    {

        private readonly RollbarConfig _config;
        private readonly HttpClient _httpClient;

        public RollbarDeployClient(RollbarConfig config, HttpClient httpClient = null)
        {
            Assumption.AssertNotNull(config, nameof(config));

            this._config = config;

            this._httpClient = httpClient;

            //var header = new MediaTypeWithQualityHeaderValue("application/json");
            //if (!this._httpClient.DefaultRequestHeaders.Accept.Contains(header))
            //{
            //    this._httpClient.DefaultRequestHeaders.Accept.Add(header);
            //}

            var sp = ServicePointManager.FindServicePoint(new Uri(this._config.EndPoint));
            sp.ConnectionLeaseTimeout = 60 * 1000; // 1 minute
        }

        public RollbarConfig Config { get { return this._config; } }

        private HttpClient ProvideHttpClient()
        {
            if (this._httpClient != null)
            {
                return this._httpClient;
            }

            return HttpClientUtil.CreateHttpClient(this._config.ProxyAddress);
        }

        private bool Release(HttpClient httpClient)
        {
            Assumption.AssertNotNull(httpClient, nameof(httpClient));

            if (httpClient == this._httpClient)
            {
                // client code controls life-time of the http client:
                return false;
            }

            httpClient.Dispose();
            return true;
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

            var uri = new Uri(this._config.EndPoint + RollbarDeployClient.deployApiPath);

            var parameters = new Dictionary<string, string> {
                    { "access_token", this._config.AccessToken },
                    { "environment", (!string.IsNullOrWhiteSpace(deployment.Environment)) ? deployment.Environment : this._config.Environment  },
                    { "revision", deployment.Revision },
                    { "rollbar_username", deployment.RollbarUsername },
                    { "local_username", deployment.LocalUsername },
                    { "comment", deployment.Comment },
                };

            var httpContent = new FormUrlEncodedContent(parameters);

            var httpClient = ProvideHttpClient();
            var postResponse = await httpClient.PostAsync(uri, httpContent);

            if (postResponse.IsSuccessStatusCode)
            {
                string reply = await postResponse.Content.ReadAsStringAsync();
            }
            else
            {
                postResponse.EnsureSuccessStatusCode();
            }

            this.Release(httpClient);

            return;
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

            var uri = new Uri(
                this._config.EndPoint
                + RollbarDeployClient.deployApiPath + deploymentID + @"/"
                + $"?access_token={readAccessToken}"
                );

            var httpClient = ProvideHttpClient();
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

            this.Release(httpClient);

            return response;
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

            var uri = new Uri(
                this._config.EndPoint
                + RollbarDeployClient.deploysQueryApiPath
                + $"?access_token={readAccessToken}&page={pageNumber}"
                );

            var httpClient = ProvideHttpClient();
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

            this.Release(httpClient);

            return response;
        }

        #endregion deployment
    }
}
