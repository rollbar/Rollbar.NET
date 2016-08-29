using System;
using System.Net;
#if NETFX_45
using System.Threading.Tasks;
#endif
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

#if NETFX_45
        public async Task<Guid> PostItemAsync(Payload payload)
        {
            var stringResult = await SendPostAsync("item/", payload);
            return ParseResponse(stringResult);
        }
#endif

        private static Guid ParseResponse(string stringResult)
        {
            var response = JsonConvert.DeserializeObject<RollbarResponse>(stringResult);
            return new Guid(Convert.ToString(response.Result.Uuid));
        }

        private string SendPost<T>(string url, T payload)
        {
            var webClient = new WebClient();
            return webClient.UploadString(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
        }

#if NETFX_45
        private async Task<string> SendPostAsync<T>(string url, T payload)
        {
            var webClient = new WebClient(); 
            return await webClient.UploadStringTaskAsync(new Uri($"{Config.EndPoint}{url}"), JsonConvert.SerializeObject(payload));
        }
#endif
    }
}
