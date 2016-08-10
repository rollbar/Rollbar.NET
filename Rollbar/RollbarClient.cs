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
            var webClient = new WebClient();
            return webClient.UploadString(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
        }

        private async Task<string> SendPostAsync<T>(string url, T payload)
        {
            var webClient = new WebClient(); 
            return await webClient.UploadStringTaskAsync(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
        }
    }
}
