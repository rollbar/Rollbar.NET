using Newtonsoft.Json;

namespace RollbarDotNet 
{
    public class Person 
    {
        public Person(string id) 
        {
            Id = id;
        }
        
        public Person(string id, string userName, string email) 
        {
            Id = id;
            UserName = userName;
            Email = email;
        }

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Email { get; set; }
    }
}
