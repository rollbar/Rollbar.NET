using Newtonsoft.Json;

namespace RollbarDotNet {
    public class Person {
        public Person(string id) {
            Id = id;
        }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Email { get; set; }
    }
}
