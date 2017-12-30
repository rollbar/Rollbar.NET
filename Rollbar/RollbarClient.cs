using System;
using System.Net;
using System.Net.Http;
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
            // note: HttpClient lacks non-async operations
            return SendPostAsync(url, payload).Result;
        }

        private async Task<string> SendPostAsync<T>(string url, T payload)
        {
            using (var http = BuildWebClient())
            {
                var serialized = JsonConvert.SerializeObject(payload);
                var body = new StringContent(serialized);
                var response = await http.PostAsync(new Uri($"{Config.EndPoint}{url}"), body);
                
                if (!response.IsSuccessStatusCode) throw new System.Exception(response.ReasonPhrase);
                
                return await response.Content.ReadAsStringAsync();
            }
        }

        private HttpClient BuildWebClient()
        {
            var proxy = this.BuildWebProxy();
            
            if (proxy == null) return new HttpClient();

            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
                PreAuthenticate = true,
                UseDefaultCredentials = false,
            };
            
            return new HttpClient(httpClientHandler);
        }

        private IWebProxy BuildWebProxy()
        {
            return !string.IsNullOrWhiteSpace(Config?.ProxyAddress) 
                ? new WebProxy(this.Config.ProxyAddress) 
                : null;
        }
    }
}
