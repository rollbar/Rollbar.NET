[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Rollbar.DTOs;
    using Rollbar.Diagnostics;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using Rollbar.Serialization.Json;

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    internal class RollbarClient 
    {
        public RollbarConfig Config { get; }

        public RollbarClient(RollbarConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            Config = config;
        }

        public RollbarResponse PostAsJson(Payload payload, IEnumerable<string> scrubFields)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            var jsonData = JsonConvert.SerializeObject(payload);
            jsonData = ScrubPayload(jsonData, scrubFields);

            var jsonResult = Post("item/", jsonData, payload.AccessToken);

            var response = JsonConvert.DeserializeObject<RollbarResponse>(jsonResult);
            return response;
        }

        public async Task<RollbarResponse> PostAsJsonAsync(Payload payload, IEnumerable<string> scrubFields)
        {
            return await Task.Factory.StartNew(() => this.PostAsJson(payload, scrubFields));
        }

        private string Post(string urlSuffix, string data, string accessToken)
        {
            using (var webClient = this.BuildWebClient())
            {
                webClient.Headers.Add("X-Rollbar-Access-Token", accessToken);
                return webClient.UploadString(new Uri($"{Config.EndPoint}{urlSuffix}"), data);
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

        private WebClient BuildWebClient()
        {
            var webClient = new WebClient();

            var webProxy = this.BuildWebProxy();

            if (webProxy != null)
            {
                webClient.Proxy = webProxy;
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
    }
}
