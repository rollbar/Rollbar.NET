using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RollbarDotNet
{
    /// <summary>
    /// Client for accessing the Rollbar API
    /// </summary>
    public class RollbarClient : IRollbarClient
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

        public Task<Guid> PostItemAsync(Payload payload)
        {
            var stringResult = SendPostAsync("item/", payload);            
            return Task<Guid>.Factory.StartNew(()=> ParseResponse(stringResult));
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

        private string SendPostAsync<T>(string url, T payload)
        {
            var webClient = this.BuildWebClient();
            webClient.UploadStringAsync(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
            return string.Empty;
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
            if (this.Config != null && !string.IsNullOrWhiteSpace(this.Config.ProxyAddress))
            {
                return new WebProxy(this.Config.ProxyAddress);
            }

            return null;
        }
    }
}
