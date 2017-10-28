namespace Rollbar
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Rollbar.DTOs;

    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    public class RollbarClient 
    {
        public RollbarConfig Config { get; }

        public RollbarClient(RollbarConfig config)
        {
            Config = config;
        }

        public Guid PostItem(Payload payload)
        {
            var stringResult = SendPost("item/", payload);
            return ParseResponse(stringResult);
        }

        public async Task<Guid> PostItemAsync(Payload payload)
        {
            var stringResult = await SendPostAsync("item/", payload);
            return ParseResponse(stringResult);
        }

        private static Guid ParseResponse(string stringResult)
        {
            var response = JsonConvert.DeserializeObject<RollbarResponse>(stringResult);
            return Guid.Parse(response.Result.Uuid);
        }

        private string SendPost<T>(string url, T payload)
        {
            var webClient = this.BuildWebClient();
            return webClient.UploadString(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
        }

        private async Task<string> SendPostAsync<T>(string url, T payload)
        {
            var webClient = this.BuildWebClient();
            return await webClient.UploadStringTaskAsync(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
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
